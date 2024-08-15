#if NETSTANDARD2_0
using System;

namespace FastDtw.CSharp
{
    public static partial class Dtw
    {
        public static double GetScore(double[] arrayA, double[] arrayB)
        {
            Shared.ValidateLength(arrayA, arrayB);
            var aLength = arrayA.Length;
            var bLength = arrayB.Length;
            var tCostMatrix = new double[2 * bLength];

            var previousRow = 0;
            var currentRow = -bLength;
            var tPathLength = 2 * bLength;

            double lastMin;
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

                        for (int j = 0; j < bLength; j++)
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
                                lastMin = pArrayCost[previousRow + j];
                            }
                            else
                            {
                                lastMin = Shared.FindMinimum(
                                    ref pArrayCost[previousRow + j],
                                    ref pArrayCost[previousRow + j - 1],
                                    ref lastCalculatedCost);
                            }

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

        public static float GetScoreF(float[] arrayA, float[] arrayB)
        {
            Shared.ValidateLength(arrayA, arrayB);
            var aLength = arrayA.Length;
            var bLength = arrayB.Length;
            var tCostMatrix = new float[2 * bLength];

            var previousRow = 0;
            var currentRow = -bLength;
            var tPathLength = 2 * bLength;

            float lastMin;
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
                                lastMin = pArrayCost[previousRow + j];
                            }
                            else
                            {
                                lastMin = Shared.FindMinimumF(
                                    ref pArrayCost[previousRow + j],
                                    ref pArrayCost[previousRow + j - 1],
                                    ref lastCalculatedCost);
                            }

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
#endif