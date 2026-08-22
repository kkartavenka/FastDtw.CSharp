using System;
using FastDtw.CSharp.Implementations;

namespace FastDtw.CSharp
{
    /// <summary>
    /// Constrained, batch and search entry points. Everything here is additive — the existing
    /// <see cref="Dtw.GetScore(double[], double[])"/>, weighting, normalization and path APIs are
    /// unchanged.
    /// </summary>
    public static partial class Dtw
    {
        /// <summary>
        /// DTW distance constrained to a Sakoe-Chiba band, in O(n * bandWidth) instead of O(n * m).
        /// <para>This is a different distance from the unconstrained overload: the warping path may
        /// not stray further than <paramref name="bandWidth"/> from the diagonal, so the result is
        /// greater than or equal to <see cref="GetScore(double[], double[])"/>. The band is widened
        /// automatically by the length difference so a path always exists.</para>
        /// </summary>
        public static double GetScore(double[] arrayA, double[] arrayB, int bandWidth)
        {
            return UnweightedDtwBanded.GetScore(arrayA, arrayB, bandWidth);
        }

        /// <inheritdoc cref="GetScore(double[], double[], int)"/>
        public static float GetScore(float[] arrayA, float[] arrayB, int bandWidth)
        {
            return UnweightedDtwBanded.GetScoreF(arrayA, arrayB, bandWidth);
        }

#if NET6_0_OR_GREATER
        /// <summary>
        /// DTW distance from one query to every subject. Subjects of equal length are scored several
        /// at a time in SIMD lanes and chunks run in parallel, so this is substantially faster than
        /// calling <see cref="GetScore(double[], double[])"/> in a loop. Results are identical.
        /// </summary>
        public static double[] GetScores(double[] query, double[][] subjects)
        {
            ValidateBatch(query, subjects);
            return DtwBatch.GetScores(query, subjects);
        }

        /// <inheritdoc cref="GetScores(double[], double[][])"/>
        public static float[] GetScores(float[] query, float[][] subjects)
        {
            ValidateBatch(query, subjects);
            return DtwBatch.GetScoresF(query, subjects);
        }

        /// <summary>
        /// Full symmetric all-pairs distance matrix. The diagonal is zero and <c>[i,j] == [j,i]</c>.
        /// </summary>
        public static double[,] GetScoreMatrix(double[][] series)
        {
            ValidateSeries(series);
            return DtwBatch.GetScoreMatrix(series);
        }

        /// <inheritdoc cref="GetScoreMatrix(double[][])"/>
        public static float[,] GetScoreMatrix(float[][] series)
        {
            ValidateSeries(series);
            return DtwBatch.GetScoreMatrixF(series);
        }
#endif

        /// <summary>
        /// LB_Keogh lower bound on the <em>banded</em> DTW distance for the same band width.
        /// <para>It does not lower-bound the unconstrained distance — relaxing the band can only
        /// lower the result — so it is only safe to prune a banded search with it. Both series must
        /// have the same length.</para>
        /// </summary>
        public static double GetLowerBound(double[] query, double[] candidate, int bandWidth)
        {
            return LbKeogh.GetLowerBound(query, candidate, bandWidth);
        }

        /// <inheritdoc cref="GetLowerBound(double[], double[], int)"/>
        public static float GetLowerBound(float[] query, float[] candidate, int bandWidth)
        {
            return LbKeogh.GetLowerBoundF(query, candidate, bandWidth);
        }

        /// <summary>
        /// Index of the subject with the smallest banded DTW distance to the query, using LB_Keogh to
        /// skip candidates that cannot win. The result is identical to scoring every subject with
        /// <see cref="GetScore(double[], double[], int)"/> and taking the minimum — the bound only
        /// removes candidates whose distance is provably no better than the incumbent.
        /// <para>Returns -1 for an empty subject set. All series must share one length.</para>
        /// </summary>
        public static int GetNearestNeighbor(double[] query, double[][] subjects, int bandWidth, out double distance)
        {
            return LbKeogh.GetNearestNeighbor(query, subjects, bandWidth, out distance);
        }

        /// <inheritdoc cref="GetNearestNeighbor(double[], double[][], int, out double)"/>
        public static int GetNearestNeighbor(float[] query, float[][] subjects, int bandWidth, out float distance)
        {
            return LbKeogh.GetNearestNeighborF(query, subjects, bandWidth, out distance);
        }

        private static void ValidateBatch<T>(T[] query, T[][] subjects) where T : struct
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            ValidateSeries(subjects);
        }

        private static void ValidateSeries<T>(T[][] series) where T : struct
        {
            if (series == null)
            {
                throw new ArgumentNullException(nameof(series));
            }

            for (var i = 0; i < series.Length; i++)
            {
                if (series[i] == null)
                {
                    throw new ArgumentException($"Series at index {i} is null", nameof(series));
                }
            }
        }
    }
}
