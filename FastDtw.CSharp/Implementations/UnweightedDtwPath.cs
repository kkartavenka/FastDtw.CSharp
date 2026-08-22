using System;
using System.Collections.Generic;
using FastDtw.CSharp.Implementations.Shared;

namespace FastDtw.CSharp.Implementations
{
    public static class UnweightedDtwPath
    {

        private const byte StepDiagonal = 0;
        private const byte StepUp = 1;
        private const byte StepLeft = 2;

        /// <summary>
        /// Warping path and score.
        /// <para>The cost matrix itself is not retained: the backtrace only ever needs to know which
        /// of the three predecessors won, so the forward pass records one byte per cell and keeps
        /// just two rolling rows of costs. That is 8x less memory than storing the full
        /// <c>double[n, m]</c>, and less memory traffic makes it faster as well.</para>
        /// <para>Tie-breaking is identical to <see cref="GetPathDense"/>, so the returned path is the
        /// same one, cell for cell.</para>
        /// </summary>
        public static PathResult GetPath(double[] arrayA, double[] arrayB)
        {
            InputArrayValidator.ValidateLength(arrayA, arrayB);

            var aLength = arrayA.Length;
            var bLength = arrayB.Length;

            var steps = new byte[(long)aLength * bLength];
            var previous = new double[bLength];
            var current = new double[bLength];

            // Row 0 and column 0 are peeled out of the hot loop. Testing i == 0 and j == 0 on every
            // one of the n*m cells costs more than handling the two edges separately.
            var valueFirst = arrayA[0];
            var lastCalculatedCost = Math.Abs(valueFirst - arrayB[0]);
            previous[0] = lastCalculatedCost;
            steps[0] = StepDiagonal;

            for (var j = 1; j < bLength; j++)
            {
                lastCalculatedCost = Math.Abs(valueFirst - arrayB[j]) + lastCalculatedCost;
                previous[j] = lastCalculatedCost;
                steps[j] = StepLeft;
            }

            for (var i = 1; i < aLength; i++)
            {
                var valueA = arrayA[i];
                var rowOffset = (long)i * bLength;

                // Column 0: only the cell above is reachable.
                lastCalculatedCost = Math.Abs(valueA - arrayB[0]) + previous[0];
                current[0] = lastCalculatedCost;
                steps[rowOffset] = StepUp;

                for (var j = 1; j < bLength; j++)
                {
                    var up = previous[j];
                    var diagonal = previous[j - 1];
                    var left = lastCalculatedCost;

                    double lastMin;
                    byte step;

                    // Same preference order as the dense backtrace: diagonal, then up, then left.
                    if (diagonal <= up)
                    {
                        if (diagonal <= left) { lastMin = diagonal; step = StepDiagonal; }
                        else { lastMin = left; step = StepLeft; }
                    }
                    else
                    {
                        if (up <= left) { lastMin = up; step = StepUp; }
                        else { lastMin = left; step = StepLeft; }
                    }

                    lastCalculatedCost = Math.Abs(valueA - arrayB[j]) + lastMin;
                    current[j] = lastCalculatedCost;
                    steps[rowOffset + j] = step;
                }

                var swap = previous;
                previous = current;
                current = swap;
            }

            return new PathResult(previous[bLength - 1], BuildPath(steps, aLength, bLength));
        }

        private static List<Tuple<int, int>> BuildPath(byte[] steps, int aLength, int bLength)
        {
            var result = new List<Tuple<int, int>>(aLength + bLength);
            var i = aLength - 1;
            var j = bLength - 1;

            result.Add(Tuple.Create(i, j));
            while (i != 0 || j != 0)
            {
                switch (steps[(long)i * bLength + j])
                {
                    case StepUp:
                        i--;
                        break;
                    case StepLeft:
                        j--;
                        break;
                    default:
                        i--;
                        j--;
                        break;
                }

                result.Add(Tuple.Create(i, j));
            }

            result.Reverse();
            return result;
        }

        internal static PathResult GetPathDense(double[] arrayA, double[] arrayB)
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