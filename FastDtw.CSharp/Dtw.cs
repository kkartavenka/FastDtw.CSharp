#if NET6_0_OR_GREATER
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace FastDtw.CSharp;

public static partial class Dtw
{
    public static double GetScore(Span<double> arrayA, Span<double> arrayB)
    {
        Shared.ValidateLength(arrayA, arrayB);

        var aLength = arrayA.Length;
        var bLength = arrayB.Length;
        var tCostMatrix = new double[2 * bLength];

        ref var arrayAZeroElement = ref MemoryMarshal.GetReference(arrayA);
        ref var arrayBZeroElement = ref MemoryMarshal.GetReference(arrayB);
        ref var costMatrixZeroElement = ref MemoryMarshal.GetArrayDataReference(tCostMatrix);

        var previousRow = 0;
        var currentRow = -bLength;
        var tPathLength = tCostMatrix.Length;

        double lastMin;
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
                if (i == 0 && j == 0)
                {
                    lastMin = 0;
                }
                else if (i == 0)
                {
                    lastMin = lastCalculatedCost;
                }
                else if (j == 0)
                {
                    lastMin = Unsafe.Add(ref costMatrixZeroElement, previousRow);
                }
                else
                {
                    lastMin = Shared.FindMinimum(
                        ref Unsafe.Add(ref costMatrixZeroElement, previousRow + j),
                        ref Unsafe.Add(ref costMatrixZeroElement, previousRow + j - 1),
                        ref lastCalculatedCost);
                }

                var absDifference =
                    Math.Abs(Unsafe.Add(ref arrayAZeroElement, i) - Unsafe.Add(ref arrayBZeroElement, j));

                lastCalculatedCost = absDifference + lastMin;
                Unsafe.Add(ref costMatrixZeroElement, currentRow + j) = lastCalculatedCost;
            }

            previousRow = currentRow;
        }

        return Unsafe.Add(ref costMatrixZeroElement, currentRow + bLength - 1);
    }

    public static double GetScore(double[] arrayA, double[] arrayB)
    {
        return GetScore(arrayA.AsSpan(), arrayB.AsSpan());
    }

    public static float GetScoreF(Span<float> arrayA, Span<float> arrayB)
    {
        Shared.ValidateLength(arrayA, arrayB);

        var aLength = arrayA.Length;
        var bLength = arrayB.Length;
        var tCostMatrix = new float[2 * bLength];

        ref var arrayAZeroElement = ref MemoryMarshal.GetReference(arrayA);
        ref var arrayBZeroElement = ref MemoryMarshal.GetReference(arrayB);
        ref var costMatrixZeroElement = ref MemoryMarshal.GetArrayDataReference(tCostMatrix);

        var previousRow = 0;
        var currentRow = -bLength;
        var tPathLength = tCostMatrix.Length;

        float lastMin;
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
                if (i == 0 && j == 0)
                {
                    lastMin = 0;
                }
                else if (i == 0)
                {
                    lastMin = lastCalculatedCost;
                }
                else if (j == 0)
                {
                    lastMin = Unsafe.Add(ref costMatrixZeroElement, previousRow + j);
                }
                else
                {
                    lastMin = Shared.FindMinimumF(
                        ref Unsafe.Add(ref costMatrixZeroElement, previousRow + j),
                        ref Unsafe.Add(ref costMatrixZeroElement, previousRow + j - 1),
                        ref lastCalculatedCost);
                }

                var absDifference =
                    Math.Abs(Unsafe.Add(ref arrayAZeroElement, i) - Unsafe.Add(ref arrayBZeroElement, j));

                lastCalculatedCost = absDifference + lastMin;
                Unsafe.Add(ref costMatrixZeroElement, currentRow + j) = lastCalculatedCost;
            }

            previousRow = currentRow;
        }

        return Unsafe.Add(ref costMatrixZeroElement, currentRow + bLength - 1);
    }

    public static float GetScoreF(float[] arrayA, float[] arrayB)
    {
        return GetScoreF(arrayA.AsSpan(), arrayB.AsSpan());
    }
}
#endif