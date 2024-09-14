using System;
using FastDtw.CSharp.Implementations.Shared;

namespace FastDtw.CSharp.Implementations
{
    public static class UnweightedDtwPath
    {
        public static PathResult GetPath(double[] arrayA, double[] arrayB)
        {
            var aLength = arrayA.Length;
            var bLength = arrayB.Length;
            var tCostMatrix = new double[aLength, bLength];

            double lastMin;
            double lastCalculatedCost = 0;
            unsafe
            {
                fixed (double* pArrayA = arrayA, pArrayB = arrayB, pArrayCost = tCostMatrix)
                {
                    for (var i = 0; i < aLength; i++)
                    {
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
                                lastMin = pArrayCost[(i - 1) * bLength];
                            }
                            else
                            {
                                lastMin = NumericHelpers.FindMinimum(
                                    ref pArrayCost[(i - 1) * bLength + j],
                                    ref pArrayCost[(i - 1) * bLength + j - 1],
                                    ref lastCalculatedCost);
                            }

                            var absDifference = Math.Abs(pArrayA[i] - pArrayB[j]);

                            lastCalculatedCost = absDifference + lastMin;
                            pArrayCost[i * bLength + j] = lastCalculatedCost;
                        }
                    }
                }
            }

            return new PathResult(tCostMatrix[aLength - 1, bLength - 1],
                DtwShared.GetPathFromCostMatrix(tCostMatrix, aLength, bLength));
        }
    }
}