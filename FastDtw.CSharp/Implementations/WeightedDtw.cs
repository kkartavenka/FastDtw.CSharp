#if NET6_0_OR_GREATER
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using FastDtw.CSharp.Implementations.Shared;

namespace FastDtw.CSharp.Implementations;

internal static class WeightedDtw
{
    internal static double GetScore(
        Span<double> arrayA,
        Span<double> arrayB,
        Span<double> weightsA,
        Span<double> weightsB,
        WeightingApproach weightingApproach)
    {
        InputArrayValidator.ValidateLength(arrayA, arrayB, weightsA, weightsB);

        var aLength = arrayA.Length;
        var bLength = arrayB.Length;
        var tCostMatrix = new double[2 * bLength];

        ref var arrayAZeroElement = ref MemoryMarshal.GetReference(arrayA);
        ref var arrayBZeroElement = ref MemoryMarshal.GetReference(arrayB);
        ref var arrayAWeightsElement = ref MemoryMarshal.GetReference(weightsA);
        ref var arrayBWeightsElement = ref MemoryMarshal.GetReference(weightsB);
        ref var costMatrixZeroElement = ref MemoryMarshal.GetArrayDataReference(tCostMatrix);

        var previousRow = 0;
        var currentRow = -bLength;
        var tPathLength = tCostMatrix.Length;

        double lastMin = 0;
        double lastCalculatedCost = 0;
        for (var i = 0; i < aLength; i++)
        {
            currentRow += bLength;
            if (currentRow == tPathLength)
            {
                currentRow = 0;
            }

            for (var j = 0; j < bLength; j++)
            {
                DtwShared.UpdateLastMin(i, j, previousRow, lastCalculatedCost, ref costMatrixZeroElement, ref lastMin);

                var weight = WeightHelpers.GetWeight(
                    Unsafe.Add(ref arrayAWeightsElement, i),
                    Unsafe.Add(ref arrayBWeightsElement, j),
                    weightingApproach);

                var absDifference =
                    weight * Math.Abs(Unsafe.Add(ref arrayAZeroElement, i) - Unsafe.Add(ref arrayBZeroElement, j));

                lastCalculatedCost = absDifference + lastMin;
                Unsafe.Add(ref costMatrixZeroElement, currentRow + j) = lastCalculatedCost;
            }

            previousRow = currentRow;
        }

        return Unsafe.Add(ref costMatrixZeroElement, currentRow + bLength - 1);
    }
    
    internal static float GetScoreF(
        Span<float> arrayA,
        Span<float> arrayB,
        Span<float> weightsA,
        Span<float> weightsB,
        WeightingApproach weightingApproach)
    {
        InputArrayValidator.ValidateLength(arrayA, arrayB, weightsA, weightsB);

        var aLength = arrayA.Length;
        var bLength = arrayB.Length;
        var tCostMatrix = new float[2 * bLength];

        ref var arrayAZeroElement = ref MemoryMarshal.GetReference(arrayA);
        ref var arrayBZeroElement = ref MemoryMarshal.GetReference(arrayB);
        ref var arrayAWeightsElement = ref MemoryMarshal.GetReference(weightsA);
        ref var arrayBWeightsElement = ref MemoryMarshal.GetReference(weightsB);
        ref var costMatrixZeroElement = ref MemoryMarshal.GetArrayDataReference(tCostMatrix);

        var previousRow = 0;
        var currentRow = -bLength;
        var tPathLength = tCostMatrix.Length;

        float lastMin = 0;
        float lastCalculatedCost = 0;
        for (var i = 0; i < aLength; i++)
        {
            currentRow += bLength;
            if (currentRow == tPathLength)
            {
                currentRow = 0;
            }

            for (var j = 0; j < bLength; j++)
            {
                DtwShared.UpdateLastMinF(i, j, previousRow, lastCalculatedCost, ref costMatrixZeroElement, ref lastMin);

                var weight = WeightHelpers.GetWeightF(
                    Unsafe.Add(ref arrayAWeightsElement, i),
                    Unsafe.Add(ref arrayBWeightsElement, j),
                    weightingApproach);

                var absDifference =
                    weight * MathF.Abs(Unsafe.Add(ref arrayAZeroElement, i) - Unsafe.Add(ref arrayBZeroElement, j));

                lastCalculatedCost = absDifference + lastMin;
                Unsafe.Add(ref costMatrixZeroElement, currentRow + j) = lastCalculatedCost;
            }

            previousRow = currentRow;
        }

        return Unsafe.Add(ref costMatrixZeroElement, currentRow + bLength - 1);
    }
}
#endif