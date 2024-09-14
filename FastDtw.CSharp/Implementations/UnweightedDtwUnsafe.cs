using System;
using FastDtw.CSharp.Implementations.Shared;

namespace FastDtw.CSharp.Implementations
{
    internal static class UnweightedDtwUnsafe
    {
        internal static double GetScore(double[] arrayA, double[] arrayB)
        {
            InputArrayValidator.ValidateLength(arrayA, arrayB);
            var aLength = arrayA.Length;
            var bLength = arrayB.Length;
            var tCostMatrix = new double[2 * bLength];

            var previousRow = 0;
            var currentRow = -bLength;
            var tPathLength = 2 * bLength;

            double lastMin = 0;
            double lastCalculatedCost = 0;

            unsafe
            {
                fixed (double* pArrayA = arrayA, pArrayB = arrayB, pArrayCost = tCostMatrix)
                {
                    for (var i = 0; i < aLength; i++)
                    {
                        currentRow += bLength;
                        if (currentRow == tPathLength)
                        {
                            currentRow = 0;
                        }

                        for (var j = 0; j < bLength; j++)
                        {
                            DtwShared.UpdateLastMin(i, j, previousRow, lastCalculatedCost, pArrayCost, ref lastMin);

                            var absDifference = Math.Abs(pArrayA[i] - pArrayB[j]);

                            lastCalculatedCost = absDifference + lastMin;
                            pArrayCost[currentRow + j] = lastCalculatedCost;
                        }

                        previousRow = currentRow;
                    }
                }

                return tCostMatrix[currentRow + bLength - 1];
            }
        }

        internal static float GetScoreF(float[] arrayA, float[] arrayB)
        {
            InputArrayValidator.ValidateLength(arrayA, arrayB);
            var aLength = arrayA.Length;
            var bLength = arrayB.Length;
            var tCostMatrix = new float[2 * bLength];

            var previousRow = 0;
            var currentRow = -bLength;
            var tPathLength = 2 * bLength;

            float lastMin = 0;
            float lastCalculatedCost = 0;

            unsafe
            {
                fixed (float* pArrayA = arrayA, pArrayB = arrayB, pArrayCost = tCostMatrix)
                {
                    for (var i = 0; i < aLength; i++)
                    {
                        currentRow += bLength;
                        if (currentRow == tPathLength)
                        {
                            currentRow = 0;
                        }

                        for (int j = 0; j < bLength; j++)
                        {
                            DtwShared.UpdateLastMinF(i, j, previousRow, lastCalculatedCost, pArrayCost, ref lastMin);

                            var absDifference = Math.Abs(pArrayA[i] - pArrayB[j]);

                            lastCalculatedCost = absDifference + lastMin;
                            pArrayCost[currentRow + j] = lastCalculatedCost;
                        }

                        previousRow = currentRow;
                    }
                }

                return tCostMatrix[currentRow + bLength - 1];
            }
        }
    }
}