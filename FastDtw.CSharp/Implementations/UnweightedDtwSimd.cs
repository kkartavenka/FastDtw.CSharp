#if NET6_0_OR_GREATER
using System;
using System.Buffers;
using System.Numerics;
using FastDtw.CSharp.Implementations.Shared;

namespace FastDtw.CSharp.Implementations
{
    /// <summary>
    /// Anti-diagonal ("skewed") formulation of the DTW recurrence.
    /// <para>Row-wise, <c>D[i][j]</c> depends on <c>D[i][j-1]</c>, so the inner loop is a serial
    /// min-min-add latency chain and no amount of unrolling helps. Cells on one anti-diagonal depend
    /// only on the two preceding anti-diagonals, which breaks that dependency and lets a whole
    /// diagonal be computed with SIMD.</para>
    /// <para>Results are bit-identical to <see cref="UnweightedDtw"/>: this reorders memory access,
    /// not arithmetic.</para>
    /// </summary>
    internal static class UnweightedDtwSimd
    {
        internal static double GetScore(double[] arrayA, double[] arrayB)
        {
            InputArrayValidator.ValidateLength(arrayA, arrayB);

            var aLength = arrayA.Length;
            var bLength = arrayB.Length;
            const double infinity = double.PositiveInfinity;
            var length = bLength + 1;

            var pool = ArrayPool<double>.Shared;
            var reversedA = pool.Rent(aLength);
            var twoBack = pool.Rent(length);
            var oneBack = pool.Rent(length);
            var current = pool.Rent(length);

            try
            {
                // A is walked backwards as the diagonal advances; reversing it once keeps both
                // operand loads contiguous and ascending.
                for (var k = 0; k < aLength; k++)
                {
                    reversedA[k] = arrayA[aLength - 1 - k];
                }

                // Rented buffers arrive dirty; clear so behaviour matches a fresh allocation.
                Array.Clear(twoBack, 0, length);
                Array.Clear(oneBack, 0, length);
                Array.Clear(current, 0, length);

                twoBack[0] = 0d;                                    // d = 0 : D[0][0]
                oneBack[0] = infinity;                              // d = 1 : D[1][0]
                oneBack[1] = infinity;                              //         D[0][1]

                var width = Vector<double>.Count;

                for (var d = 2; d <= aLength + bLength; d++)
                {
                    if (d - aLength <= 0) current[0] = infinity;    // D[d][0]
                    if (d <= bLength) current[d] = infinity;        // D[0][d]

                    var from = Math.Max(1, d - aLength);
                    var to = Math.Min(bLength, d - 1);
                    var offset = aLength - d;
                    var j = from;

                    for (; j + width - 1 <= to; j += width)
                    {
                        var up = new Vector<double>(oneBack, j);
                        var left = new Vector<double>(oneBack, j - 1);
                        var diagonal = new Vector<double>(twoBack, j - 1);
                        var cost = Vector.Abs(
                            new Vector<double>(reversedA, offset + j) - new Vector<double>(arrayB, j - 1));

                        (cost + Vector.Min(up, Vector.Min(left, diagonal))).CopyTo(current, j);
                    }

                    for (; j <= to; j++)
                    {
                        current[j] = Math.Abs(reversedA[offset + j] - arrayB[j - 1])
                                     + NumericHelpers.FindMinimum(
                                         ref oneBack[j], ref oneBack[j - 1], ref twoBack[j - 1]);
                    }

                    var spare = twoBack;
                    twoBack = oneBack;
                    oneBack = current;
                    current = spare;
                }

                return oneBack[bLength];
            }
            finally
            {
                // The rotation permutes the references; all three are still returned exactly once.
                pool.Return(reversedA);
                pool.Return(twoBack);
                pool.Return(oneBack);
                pool.Return(current);
            }
        }

        internal static float GetScoreF(float[] arrayA, float[] arrayB)
        {
            InputArrayValidator.ValidateLength(arrayA, arrayB);

            var aLength = arrayA.Length;
            var bLength = arrayB.Length;
            const float infinity = float.PositiveInfinity;
            var length = bLength + 1;

            var pool = ArrayPool<float>.Shared;
            var reversedA = pool.Rent(aLength);
            var twoBack = pool.Rent(length);
            var oneBack = pool.Rent(length);
            var current = pool.Rent(length);

            try
            {
                for (var k = 0; k < aLength; k++)
                {
                    reversedA[k] = arrayA[aLength - 1 - k];
                }

                Array.Clear(twoBack, 0, length);
                Array.Clear(oneBack, 0, length);
                Array.Clear(current, 0, length);

                twoBack[0] = 0f;
                oneBack[0] = infinity;
                oneBack[1] = infinity;

                var width = Vector<float>.Count;

                for (var d = 2; d <= aLength + bLength; d++)
                {
                    if (d - aLength <= 0) current[0] = infinity;
                    if (d <= bLength) current[d] = infinity;

                    var from = Math.Max(1, d - aLength);
                    var to = Math.Min(bLength, d - 1);
                    var offset = aLength - d;
                    var j = from;

                    for (; j + width - 1 <= to; j += width)
                    {
                        var up = new Vector<float>(oneBack, j);
                        var left = new Vector<float>(oneBack, j - 1);
                        var diagonal = new Vector<float>(twoBack, j - 1);
                        var cost = Vector.Abs(
                            new Vector<float>(reversedA, offset + j) - new Vector<float>(arrayB, j - 1));

                        (cost + Vector.Min(up, Vector.Min(left, diagonal))).CopyTo(current, j);
                    }

                    for (; j <= to; j++)
                    {
                        current[j] = Math.Abs(reversedA[offset + j] - arrayB[j - 1])
                                     + NumericHelpers.FindMinimumF(
                                         ref oneBack[j], ref oneBack[j - 1], ref twoBack[j - 1]);
                    }

                    var spare = twoBack;
                    twoBack = oneBack;
                    oneBack = current;
                    current = spare;
                }

                return oneBack[bLength];
            }
            finally
            {
                pool.Return(reversedA);
                pool.Return(twoBack);
                pool.Return(oneBack);
                pool.Return(current);
            }
        }
    }
}
#endif
