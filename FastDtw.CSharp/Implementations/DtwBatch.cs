#if NET6_0_OR_GREATER
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;

namespace FastDtw.CSharp.Implementations
{
    /// <summary>
    /// Batch scoring. Two independent gains stack here:
    /// <list type="number">
    /// <item>pairs are independent, so the work parallelises across cores;</item>
    /// <item>subjects of equal length are scored several at a time in SIMD lanes.</item>
    /// </list>
    /// <para>The lane kernel is row-wise rather than skewed: each lane's recurrence is independent,
    /// so the left-neighbour dependency costs nothing once several pairs are in flight. <c>diagonal</c>
    /// is the previous iteration's <c>up</c> and <c>left</c> is the previous iteration's result, so
    /// both stay in registers — leaving one load from the previous row, one from the interleaved
    /// subject block, and one store: half the memory traffic per cell of the skewed kernel.</para>
    /// </summary>
    internal static class DtwBatch
    {
        /// <summary>
        /// A unit of batch work. Every lane in a chunk shares one inner loop, so a vectorised chunk
        /// only ever holds subjects of equal length; leftovers become single-pair chunks.
        /// </summary>
        internal readonly struct LaneChunk
        {
            internal LaneChunk(int[] indices, bool vectorised)
            {
                Indices = indices;
                Vectorised = vectorised;
            }

            internal int[] Indices { get; }
            internal bool Vectorised { get; }
        }

        /// <summary>Groups subjects by length, then splits each group into SIMD-width chunks.</summary>
        internal static List<LaneChunk> PlanLanes<T>(T[][] subjects, int width)
        {
            var byLength = new Dictionary<int, List<int>>();
            for (var i = 0; i < subjects.Length; i++)
            {
                if (!byLength.TryGetValue(subjects[i].Length, out var bucket))
                {
                    bucket = new List<int>();
                    byLength[subjects[i].Length] = bucket;
                }

                bucket.Add(i);
            }

            var plan = new List<LaneChunk>();
            foreach (var bucket in byLength.Values)
            {
                var position = 0;
                for (; position + width <= bucket.Count; position += width)
                {
                    plan.Add(new LaneChunk(bucket.GetRange(position, width).ToArray(), true));
                }

                for (; position < bucket.Count; position++)
                {
                    plan.Add(new LaneChunk(new[] { bucket[position] }, false));
                }
            }

            return plan;
        }

        internal static double[] GetScores(double[] query, double[][] subjects)
        {
            var results = new double[subjects.Length];
            var plan = PlanLanes(subjects, Vector<double>.Count);
            Parallel.ForEach(plan, chunk => ScoreChunk(query, subjects, chunk, results));
            return results;
        }

        internal static float[] GetScoresF(float[] query, float[][] subjects)
        {
            var results = new float[subjects.Length];
            var plan = PlanLanes(subjects, Vector<float>.Count);
            Parallel.ForEach(plan, chunk => ScoreChunkF(query, subjects, chunk, results));
            return results;
        }

        internal static double[,] GetScoreMatrix(double[][] series)
        {
            var count = series.Length;
            var matrix = new double[count, count];
            var width = Vector<double>.Count;

            Parallel.For(0, count, i =>
            {
                if (i + 1 >= count) return;

                var subjects = new double[count - i - 1][];
                Array.Copy(series, i + 1, subjects, 0, subjects.Length);

                var row = new double[subjects.Length];
                foreach (var chunk in PlanLanes(subjects, width))
                {
                    ScoreChunk(series[i], subjects, chunk, row);
                }

                for (var k = 0; k < row.Length; k++)
                {
                    matrix[i, i + 1 + k] = row[k];
                }
            });

            Mirror(matrix, count);
            return matrix;
        }

        internal static float[,] GetScoreMatrixF(float[][] series)
        {
            var count = series.Length;
            var matrix = new float[count, count];
            var width = Vector<float>.Count;

            Parallel.For(0, count, i =>
            {
                if (i + 1 >= count) return;

                var subjects = new float[count - i - 1][];
                Array.Copy(series, i + 1, subjects, 0, subjects.Length);

                var row = new float[subjects.Length];
                foreach (var chunk in PlanLanes(subjects, width))
                {
                    ScoreChunkF(series[i], subjects, chunk, row);
                }

                for (var k = 0; k < row.Length; k++)
                {
                    matrix[i, i + 1 + k] = row[k];
                }
            });

            Mirror(matrix, count);
            return matrix;
        }

        private static void Mirror<T>(T[,] matrix, int count)
        {
            for (var i = 0; i < count; i++)
            {
                for (var j = i + 1; j < count; j++)
                {
                    matrix[j, i] = matrix[i, j];
                }
            }
        }

        internal static void ScoreChunk(double[] query, double[][] subjects, LaneChunk chunk, double[] results)
        {
            if (!chunk.Vectorised)
            {
                foreach (var index in chunk.Indices)
                {
                    results[index] = UnweightedDtwSimd.GetScore(query, subjects[index]);
                }

                return;
            }

            var width = Vector<double>.Count;
            var subjectLength = subjects[chunk.Indices[0]].Length;
            var queryLength = query.Length;
            var length = (subjectLength + 1) * width;

            var pool = ArrayPool<double>.Shared;
            var interleaved = pool.Rent(subjectLength * width);
            var previous = pool.Rent(length);
            var current = pool.Rent(length);

            try
            {
                // Lane-major layout: interleaved[j * width + k] is subject k at position j, so one
                // vector load yields column j across every lane.
                for (var k = 0; k < width; k++)
                {
                    var subject = subjects[chunk.Indices[k]];
                    for (var j = 0; j < subjectLength; j++)
                    {
                        interleaved[j * width + k] = subject[j];
                    }
                }

                var infinity = new Vector<double>(double.PositiveInfinity);

                Array.Clear(previous, 0, width);                     // D[0][0] = 0 in every lane
                for (var j = 1; j <= subjectLength; j++)             // D[0][j > 0] = +inf
                {
                    infinity.CopyTo(previous, j * width);
                }

                for (var i = 1; i <= queryLength; i++)
                {
                    infinity.CopyTo(current, 0);                     // D[i][0] = +inf
                    var valueA = new Vector<double>(query[i - 1]);   // scalar broadcast, no gather
                    var diagonal = new Vector<double>(previous, 0);
                    var left = infinity;

                    for (var j = 1; j <= subjectLength; j++)
                    {
                        var up = new Vector<double>(previous, j * width);
                        var cost = Vector.Abs(valueA - new Vector<double>(interleaved, (j - 1) * width));
                        var value = cost + Vector.Min(up, Vector.Min(left, diagonal));
                        value.CopyTo(current, j * width);
                        diagonal = up;
                        left = value;
                    }

                    var spare = previous;
                    previous = current;
                    current = spare;
                }

                for (var k = 0; k < width; k++)
                {
                    results[chunk.Indices[k]] = previous[subjectLength * width + k];
                }
            }
            finally
            {
                pool.Return(interleaved);
                pool.Return(previous);
                pool.Return(current);
            }
        }

        internal static void ScoreChunkF(float[] query, float[][] subjects, LaneChunk chunk, float[] results)
        {
            if (!chunk.Vectorised)
            {
                foreach (var index in chunk.Indices)
                {
                    results[index] = UnweightedDtwSimd.GetScoreF(query, subjects[index]);
                }

                return;
            }

            var width = Vector<float>.Count;
            var subjectLength = subjects[chunk.Indices[0]].Length;
            var queryLength = query.Length;
            var length = (subjectLength + 1) * width;

            var pool = ArrayPool<float>.Shared;
            var interleaved = pool.Rent(subjectLength * width);
            var previous = pool.Rent(length);
            var current = pool.Rent(length);

            try
            {
                for (var k = 0; k < width; k++)
                {
                    var subject = subjects[chunk.Indices[k]];
                    for (var j = 0; j < subjectLength; j++)
                    {
                        interleaved[j * width + k] = subject[j];
                    }
                }

                var infinity = new Vector<float>(float.PositiveInfinity);

                Array.Clear(previous, 0, width);
                for (var j = 1; j <= subjectLength; j++)
                {
                    infinity.CopyTo(previous, j * width);
                }

                for (var i = 1; i <= queryLength; i++)
                {
                    infinity.CopyTo(current, 0);
                    var valueA = new Vector<float>(query[i - 1]);
                    var diagonal = new Vector<float>(previous, 0);
                    var left = infinity;

                    for (var j = 1; j <= subjectLength; j++)
                    {
                        var up = new Vector<float>(previous, j * width);
                        var cost = Vector.Abs(valueA - new Vector<float>(interleaved, (j - 1) * width));
                        var value = cost + Vector.Min(up, Vector.Min(left, diagonal));
                        value.CopyTo(current, j * width);
                        diagonal = up;
                        left = value;
                    }

                    var spare = previous;
                    previous = current;
                    current = spare;
                }

                for (var k = 0; k < width; k++)
                {
                    results[chunk.Indices[k]] = previous[subjectLength * width + k];
                }
            }
            finally
            {
                pool.Return(interleaved);
                pool.Return(previous);
                pool.Return(current);
            }
        }
    }
}
#endif
