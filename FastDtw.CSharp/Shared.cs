using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace FastDtw.CSharp
{
    internal static class Shared
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static double FindMinimum(ref double prevRowFirst, ref double prevRowSecond, ref double currentRowSecond)
        {
            if (prevRowFirst < prevRowSecond)
            {
                return prevRowFirst < currentRowSecond ? prevRowFirst : currentRowSecond;
            }

            return prevRowSecond < currentRowSecond ? prevRowSecond : currentRowSecond;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static float FindMinimumF(ref float prevRowFirst, ref float prevRowSecond, ref float currentRowSecond)
        {
            if (prevRowFirst < prevRowSecond)
            {
                return prevRowFirst < currentRowSecond ? prevRowFirst : currentRowSecond;
            }

            return prevRowSecond < currentRowSecond ? prevRowSecond : currentRowSecond;
        }
        
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
                    
                } else if (j == 0 && i != 0)
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

        internal static void ValidateLength<T>(T[] arrayA, T[] arrayB) where T : struct
        {
            if (arrayA.Length < 2)
            {
                throw new ArgumentException("Array length, should be at least 2", nameof(arrayA));
            }

            if (arrayB.Length < 2)
            {
                throw new ArgumentException("Array length, should be at least 2", nameof(arrayB));
            }
        }
    }
}