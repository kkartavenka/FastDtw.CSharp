using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace FastDtw.CSharp.Implementations.Shared
{
    internal static class DtwShared
    {
        internal static List<Tuple<int, int>> GetPathFromCostMatrix(double[,] matrix, int aLength, int bLength)
        {
            var result = new List<Tuple<int, int>>(Math.Max(aLength, bLength));
            var i = aLength - 1;
            var j = bLength - 1;

            result.Add(Tuple.Create(i, j));
            while (i != 0 || j != 0)
            {
                if (i == 0 && j != 0)
                {
                    j--;

                }
                else if (j == 0 && i != 0)
                {
                    i--;
                }
                else
                {
                    var leftRowTop = matrix[i - 1, j];
                    var currentRowBottom = matrix[i, j - 1];
                    var leftRowBottom = matrix[i - 1, j - 1];

                    if (leftRowBottom <= leftRowTop)
                    {
                        if (leftRowBottom <= currentRowBottom)
                        {
                            i--;
                            j--;
                        }
                        else
                        {
                            j--;
                        }
                    }
                    else
                    {
                        if (leftRowTop <= currentRowBottom)
                        {
                            i--;
                        }
                        else
                        {
                            j--;
                        }
                    }
                }

                result.Add(Tuple.Create(i, j));
            }

            result.Reverse();

            return result;
        }

#if NET6_0_OR_GREATER
        internal static void UpdateLastMin(int idxArrayA, int idxArrayB, int previousRow, double lastCalculatedCost,
            ref double costMatrixZeroElement, ref double lastMin)
        {
            if (idxArrayA == 0 && idxArrayB == 0)
            {
                lastMin = 0;
            }
            else if (idxArrayA == 0)
            {
                lastMin = lastCalculatedCost;
            }
            else if (idxArrayB == 0)
            {
                lastMin = Unsafe.Add(ref costMatrixZeroElement, previousRow);
            }
            else
            {
                lastMin = NumericHelpers.FindMinimum(
                    ref Unsafe.Add(ref costMatrixZeroElement, previousRow + idxArrayB),
                    ref Unsafe.Add(ref costMatrixZeroElement, previousRow + idxArrayB - 1),
                    ref lastCalculatedCost);
            }
        }

        internal static void UpdateLastMinF(int idxArrayA, int idxArrayB, int previousRow, float lastCalculatedCost,
            ref float costMatrixZeroElement, ref float lastMin)
        {
            if (idxArrayA == 0 && idxArrayB == 0)
            {
                lastMin = 0;
            }
            else if (idxArrayA == 0)
            {
                lastMin = lastCalculatedCost;
            }
            else if (idxArrayB == 0)
            {
                lastMin = Unsafe.Add(ref costMatrixZeroElement, previousRow);
            }
            else
            {
                lastMin = NumericHelpers.FindMinimumF(
                    ref Unsafe.Add(ref costMatrixZeroElement, previousRow + idxArrayB),
                    ref Unsafe.Add(ref costMatrixZeroElement, previousRow + idxArrayB - 1),
                    ref lastCalculatedCost);
            }
        }
#endif

        internal static unsafe void UpdateLastMin(int idxArrayA, int idxArrayB, int previousRow,
            double lastCalculatedCost, double* pArrayCost, ref double lastMin)
        {
            if (idxArrayA == 0 && idxArrayB == 0)
            {
                lastMin = 0;
            }
            else if (idxArrayA == 0)
            {
                lastMin = lastCalculatedCost;
            }
            else if (idxArrayB == 0)
            {
                lastMin = pArrayCost[previousRow];
            }
            else
            {
                lastMin = NumericHelpers.FindMinimum(
                    ref pArrayCost[previousRow + idxArrayB],
                    ref pArrayCost[previousRow + idxArrayB - 1],
                    ref lastCalculatedCost);
            }
        }
        
        internal static unsafe void UpdateLastMinF(int idxArrayA, int idxArrayB, int previousRow,
            float lastCalculatedCost, float* pArrayCost, ref float lastMin)
        {
            if (idxArrayA == 0 && idxArrayB == 0)
            {
                lastMin = 0;
            }
            else if (idxArrayA == 0)
            {
                lastMin = lastCalculatedCost;
            }
            else if (idxArrayB == 0)
            {
                lastMin = pArrayCost[previousRow];
            }
            else
            {
                lastMin = NumericHelpers.FindMinimumF(
                    ref pArrayCost[previousRow + idxArrayB],
                    ref pArrayCost[previousRow + idxArrayB - 1],
                    ref lastCalculatedCost);
            }
        }
    }
}