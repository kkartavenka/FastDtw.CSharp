using System;
using System.Runtime.CompilerServices;

namespace FastDtw.CSharp.Implementations.Shared
{
    internal static class WeightHelpers
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static double GetWeight(double weightA, double weightB, WeightingApproach approach)
        {
            switch (approach)
            {
                case WeightingApproach.Multiplicative:
                    return weightA * weightB;
                case WeightingApproach.ArithmeticMean:
                    return (weightA + weightB) / 2;
                case WeightingApproach.HarmonicMean:
                    var sumOfWeights = weightA + weightB;
                    if (sumOfWeights == 0)
                    {
                        throw new ArgumentException("The sum of the weight is zero");
                    }
                    return 2 * weightA * weightB / sumOfWeights;
                default:
                    throw new ArgumentOutOfRangeException(nameof(approach), approach, null);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static float GetWeightF(float weightA, float weightB, WeightingApproach approach)
        {
            switch (approach)
            {
                case WeightingApproach.Multiplicative:
                    return weightA * weightB;
                case WeightingApproach.ArithmeticMean:
                    return (weightA + weightB) / 2f;
                case WeightingApproach.HarmonicMean:
                    return 2f * weightA * weightB / (weightA + weightB);
                default:
                    throw new ArgumentOutOfRangeException(nameof(approach), approach, null);
            }
        }
    }
}