using System;
using FastDtw.CSharp.Implementations.Shared;

namespace FastDtw.CSharp.Implementations
{
    /// <summary>
    /// Sakoe-Chiba banded DTW. Only cells within <c>bandWidth</c> of the diagonal are evaluated,
    /// turning the O(n*m) fill into O(n*bandWidth).
    /// <para>This is a different distance from the unconstrained one: the warping path may not stray
    /// further than the band from the diagonal, so the result is greater than or equal to
    /// <see cref="UnweightedDtw"/>'s. The band is widened by the length difference so that the end
    /// cell always stays reachable.</para>
    /// </summary>
    internal static class UnweightedDtwBanded
    {
        /// <summary>
        /// The band actually applied for a nominal width. LB_Keogh envelopes must be built with this
        /// same value or the bound is unsound.
        /// </summary>
        internal static int EffectiveBand(int lengthA, int lengthB, int bandWidth)
        {
            return bandWidth + Math.Abs(lengthA - lengthB);
        }

        internal static double GetScore(double[] arrayA, double[] arrayB, int bandWidth)
        {
            InputArrayValidator.ValidateLength(arrayA, arrayB);
            ValidateBand(bandWidth);

            var aLength = arrayA.Length;
            var bLength = arrayB.Length;
            var band = EffectiveBand(aLength, bLength, bandWidth);
            const double infinity = double.PositiveInfinity;

            // Two rolling rows, plus one guard cell on each side of the band.
            var previous = new double[bLength + 2];
            var current = new double[bLength + 2];

            for (var j = 0; j <= bLength + 1; j++)
            {
                previous[j] = infinity;
            }

            previous[0] = 0d;

            for (var i = 1; i <= aLength; i++)
            {
                var from = Math.Max(1, i - band);
                var to = Math.Min(bLength, i + band);

                // The rows are recycled every other iteration, so the cells flanking the band still
                // hold values from row i-2. Fencing both sides keeps the next row from reading them.
                current[from - 1] = infinity;
                current[to + 1] = infinity;

                var valueA = arrayA[i - 1];
                for (var j = from; j <= to; j++)
                {
                    var lastMin = NumericHelpers.FindMinimum(
                        ref previous[j], ref previous[j - 1], ref current[j - 1]);

                    current[j] = Math.Abs(valueA - arrayB[j - 1]) + lastMin;
                }

                var swap = previous;
                previous = current;
                current = swap;
            }

            return previous[bLength];
        }

        internal static float GetScoreF(float[] arrayA, float[] arrayB, int bandWidth)
        {
            InputArrayValidator.ValidateLength(arrayA, arrayB);
            ValidateBand(bandWidth);

            var aLength = arrayA.Length;
            var bLength = arrayB.Length;
            var band = EffectiveBand(aLength, bLength, bandWidth);
            const float infinity = float.PositiveInfinity;

            var previous = new float[bLength + 2];
            var current = new float[bLength + 2];

            for (var j = 0; j <= bLength + 1; j++)
            {
                previous[j] = infinity;
            }

            previous[0] = 0f;

            for (var i = 1; i <= aLength; i++)
            {
                var from = Math.Max(1, i - band);
                var to = Math.Min(bLength, i + band);

                current[from - 1] = infinity;
                current[to + 1] = infinity;

                var valueA = arrayA[i - 1];
                for (var j = from; j <= to; j++)
                {
                    var lastMin = NumericHelpers.FindMinimumF(
                        ref previous[j], ref previous[j - 1], ref current[j - 1]);

                    current[j] = Math.Abs(valueA - arrayB[j - 1]) + lastMin;
                }

                var swap = previous;
                previous = current;
                current = swap;
            }

            return previous[bLength];
        }

        private static void ValidateBand(int bandWidth)
        {
            if (bandWidth < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bandWidth), "Band width must not be negative");
            }
        }
    }
}
