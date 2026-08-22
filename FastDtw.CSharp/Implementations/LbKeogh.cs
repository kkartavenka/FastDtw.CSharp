using System;
using FastDtw.CSharp.Implementations.Shared;

namespace FastDtw.CSharp.Implementations
{
    /// <summary>
    /// LB_Keogh lower bound and the nearest-neighbour search built on it.
    /// <para>The bound holds against <em>banded</em> DTW with the same band. It does not lower-bound
    /// unconstrained DTW, because relaxing the band can only lower the distance — so a search pruned
    /// with it must itself be banded, which is why every entry point here takes a band width.</para>
    /// <para>The envelope radius is taken from <see cref="UnweightedDtwBanded.EffectiveBand"/>, the
    /// same value the distance uses. Deriving the two separately would make the bound unsound.</para>
    /// </summary>
    internal static class LbKeogh
    {
        internal static double GetLowerBound(double[] query, double[] candidate, int bandWidth)
        {
            InputArrayValidator.ValidateLength(query, candidate);
            // Must run before the envelope is built: a negative width yields a negative radius,
            // which inverts the window bounds and reads past the end of the series.
            ValidateBand(bandWidth);
            ValidateEqualLength(query.Length, candidate.Length);

            var radius = UnweightedDtwBanded.EffectiveBand(query.Length, candidate.Length, bandWidth);
            var envelope = BuildEnvelope(query, radius);
            return Accumulate(candidate, envelope.Upper, envelope.Lower);
        }

        internal static float GetLowerBoundF(float[] query, float[] candidate, int bandWidth)
        {
            InputArrayValidator.ValidateLength(query, candidate);
            // Must run before the envelope is built: a negative width yields a negative radius,
            // which inverts the window bounds and reads past the end of the series.
            ValidateBand(bandWidth);
            ValidateEqualLength(query.Length, candidate.Length);

            var radius = UnweightedDtwBanded.EffectiveBand(query.Length, candidate.Length, bandWidth);
            var envelope = BuildEnvelopeF(query, radius);
            return AccumulateF(candidate, envelope.Upper, envelope.Lower);
        }

        internal static int GetNearestNeighbor(double[] query, double[][] subjects, int bandWidth, out double distance)
        {
            ValidateSearch(query, subjects, bandWidth);

            var radius = UnweightedDtwBanded.EffectiveBand(query.Length, query.Length, bandWidth);
            var envelope = BuildEnvelope(query, radius);

            distance = double.PositiveInfinity;
            var best = -1;

            for (var i = 0; i < subjects.Length; i++)
            {
                // A candidate whose bound already reaches the incumbent cannot beat it.
                if (Accumulate(subjects[i], envelope.Upper, envelope.Lower) >= distance)
                {
                    continue;
                }

                var candidate = UnweightedDtwBanded.GetScore(query, subjects[i], bandWidth);
                if (candidate < distance)
                {
                    distance = candidate;
                    best = i;
                }
            }

            return best;
        }

        internal static int GetNearestNeighborF(float[] query, float[][] subjects, int bandWidth, out float distance)
        {
            ValidateSearch(query, subjects, bandWidth);

            var radius = UnweightedDtwBanded.EffectiveBand(query.Length, query.Length, bandWidth);
            var envelope = BuildEnvelopeF(query, radius);

            distance = float.PositiveInfinity;
            var best = -1;

            for (var i = 0; i < subjects.Length; i++)
            {
                if (AccumulateF(subjects[i], envelope.Upper, envelope.Lower) >= distance)
                {
                    continue;
                }

                var candidate = UnweightedDtwBanded.GetScoreF(query, subjects[i], bandWidth);
                if (candidate < distance)
                {
                    distance = candidate;
                    best = i;
                }
            }

            return best;
        }

        /// <summary>
        /// Running max/min over a window of +/- radius. Built once per query, so the direct sweep
        /// costs far less than a single distance evaluation.
        /// </summary>
        private static (double[] Upper, double[] Lower) BuildEnvelope(double[] series, int radius)
        {
            var length = series.Length;
            var upper = new double[length];
            var lower = new double[length];

            for (var i = 0; i < length; i++)
            {
                var from = Math.Max(0, i - radius);
                var to = Math.Min(length - 1, i + radius);
                var high = series[from];
                var low = series[from];

                for (var k = from + 1; k <= to; k++)
                {
                    if (series[k] > high) high = series[k];
                    if (series[k] < low) low = series[k];
                }

                upper[i] = high;
                lower[i] = low;
            }

            return (upper, lower);
        }

        private static (float[] Upper, float[] Lower) BuildEnvelopeF(float[] series, int radius)
        {
            var length = series.Length;
            var upper = new float[length];
            var lower = new float[length];

            for (var i = 0; i < length; i++)
            {
                var from = Math.Max(0, i - radius);
                var to = Math.Min(length - 1, i + radius);
                var high = series[from];
                var low = series[from];

                for (var k = from + 1; k <= to; k++)
                {
                    if (series[k] > high) high = series[k];
                    if (series[k] < low) low = series[k];
                }

                upper[i] = high;
                lower[i] = low;
            }

            return (upper, lower);
        }

        /// <summary>
        /// Sum of the candidate's excursions outside the query envelope. Absolute differences, to
        /// match this library's L1 local cost.
        /// </summary>
        private static double Accumulate(double[] candidate, double[] upper, double[] lower)
        {
            var total = 0d;
            for (var i = 0; i < candidate.Length; i++)
            {
                if (candidate[i] > upper[i]) total += candidate[i] - upper[i];
                else if (candidate[i] < lower[i]) total += lower[i] - candidate[i];
            }

            return total;
        }

        private static float AccumulateF(float[] candidate, float[] upper, float[] lower)
        {
            var total = 0f;
            for (var i = 0; i < candidate.Length; i++)
            {
                if (candidate[i] > upper[i]) total += candidate[i] - upper[i];
                else if (candidate[i] < lower[i]) total += lower[i] - candidate[i];
            }

            return total;
        }

        private static void ValidateSearch<T>(T[] query, T[][] subjects, int bandWidth) where T : struct
        {
            if (subjects == null)
            {
                throw new ArgumentNullException(nameof(subjects));
            }

            if (bandWidth < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bandWidth), "Band width must not be negative");
            }

            foreach (var subject in subjects)
            {
                if (subject == null)
                {
                    throw new ArgumentException("Subject series must not be null", nameof(subjects));
                }

                InputArrayValidator.ValidateLength(query, subject);
                ValidateEqualLength(query.Length, subject.Length);
            }
        }

        private static void ValidateBand(int bandWidth)
        {
            if (bandWidth < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bandWidth), "Band width must not be negative");
            }
        }

        private static void ValidateEqualLength(int queryLength, int candidateLength)
        {
            if (queryLength != candidateLength)
            {
                throw new ArgumentException(
                    $"LB_Keogh requires equal lengths; query is {queryLength} and candidate is {candidateLength}");
            }
        }
    }
}
