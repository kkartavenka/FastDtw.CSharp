using System;
using FastDtw.CSharp.Implementations;

namespace FastDtw.CSharp
{
    public static class Dtw
    {
#if NET6_0_OR_GREATER
        public static double GetScore(Span<double> arrayA, Span<double> arrayB)
        {
            return UnweightedDtw.GetScore(arrayA, arrayB);
        }
        
        public static float GetScore(Span<float> arrayA, Span<float> arrayB)
        {
            return UnweightedDtw.GetScoreF(arrayA, arrayB);
        }
        
        public static double GetWeightedScore(Span<double> arrayA, Span<double> arrayB, Span<double> weightsA,
            Span<double> weightsB, WeightingApproach weightingApproach)
        {
            return WeightedDtw.GetScore(arrayA, arrayB, weightsA, weightsB, weightingApproach);
        }
        
        public static float GetWeightedScore(Span<float> arrayA, Span<float> arrayB, Span<float> weightsA,
            Span<float> weightsB, WeightingApproach weightingApproach)
        {
            return WeightedDtw.GetScoreF(arrayA, arrayB, weightsA, weightsB, weightingApproach);
        }
#endif
        
        public static double GetScore(double[] arrayA, double[] arrayB)
        {
#if NET6_0_OR_GREATER
            return UnweightedDtw.GetScore(arrayA, arrayB);
#elif NETSTANDARD2_0
            return UnweightedDtwUnsafe.GetScore(arrayA, arrayB);
#endif
        }
        
        public static float GetScore(float[] arrayA, float[] arrayB)
        {
#if NET6_0_OR_GREATER
            return UnweightedDtw.GetScoreF(arrayA, arrayB);
#elif NETSTANDARD2_0
            return UnweightedDtwUnsafe.GetScoreF(arrayA, arrayB);
#endif
        }
        
        public static double GetWeightedScore(float[] arrayA, float[] arrayB, float[] weightsA, float[] weightsB,
            WeightingApproach weightingApproach)
        {
#if NET6_0_OR_GREATER
            return WeightedDtw.GetScoreF(arrayA, arrayB, weightsA, weightsB, weightingApproach);
#elif NETSTANDARD2_0
            return WeightedDtwUnsafe.GetScoreF(arrayA, arrayB, weightsA, weightsB, weightingApproach);
#endif
        }
        
        public static double GetWeightedScore(double[] arrayA, double[] arrayB, double[] weightsA, double[] weightsB,
            WeightingApproach weightingApproach)
        {
#if NET6_0_OR_GREATER
            return WeightedDtw.GetScore(arrayA, arrayB, weightsA, weightsB, weightingApproach);
#elif NETSTANDARD2_0
            return WeightedDtwUnsafe.GetScore(arrayA, arrayB, weightsA, weightsB, weightingApproach);
#endif
        }

        public static PathResult GetPath(double[] arrayA, double[] arrayB)
        {
            return UnweightedDtwPath.GetPath(arrayA, arrayB);
        }
    }
}