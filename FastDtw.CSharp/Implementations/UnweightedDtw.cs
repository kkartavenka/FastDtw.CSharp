#if NET6_0_OR_GREATER
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using FastDtw.CSharp.Implementations.Shared;

namespace FastDtw.CSharp.Implementations;

internal static class UnweightedDtw
{
    internal static double GetScore(Span<double> arrayA, Span<double> arrayB)
    {
        InputArrayValidator.ValidateLength(arrayA, arrayB);

        var aLength = arrayA.Length;
        var bLength = arrayB.Length;
        var tCostMatrix = new double[2 * bLength];

        ref var arrayAZeroElement = ref MemoryMarshal.GetReference(arrayA);
        ref var arrayBZeroElement = ref MemoryMarshal.GetReference(arrayB);
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

                var absDifference =
                    Math.Abs(Unsafe.Add(ref arrayAZeroElement, i) - Unsafe.Add(ref arrayBZeroElement, j));

                lastCalculatedCost = absDifference + lastMin;
                Unsafe.Add(ref costMatrixZeroElement, currentRow + j) = lastCalculatedCost;
            }

            previousRow = currentRow;
        }

        return Unsafe.Add(ref costMatrixZeroElement, currentRow + bLength - 1);
    }

    internal static float GetScoreF(Span<float> arrayA, Span<float> arrayB)
    {
        InputArrayValidator.ValidateLength(arrayA, arrayB);

        var aLength = arrayA.Length;
        var bLength = arrayB.Length;
        var tCostMatrix = new float[2 * bLength];

        ref var arrayAZeroElement = ref MemoryMarshal.GetReference(arrayA);
        ref var arrayBZeroElement = ref MemoryMarshal.GetReference(arrayB);
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

                var absDifference =
                    Math.Abs(Unsafe.Add(ref arrayAZeroElement, i) - Unsafe.Add(ref arrayBZeroElement, j));

                lastCalculatedCost = absDifference + lastMin;
                Unsafe.Add(ref costMatrixZeroElement, currentRow + j) = lastCalculatedCost;
            }

            previousRow = currentRow;
        }

        return Unsafe.Add(ref costMatrixZeroElement, currentRow + bLength - 1);
    }
}
#endif