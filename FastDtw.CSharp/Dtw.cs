using System;

namespace FastDtw.CSharp
{
    public static class Dtw
    {
        public static double GetScore(double[] arrayA, double[] arrayB)
        {
            var (aLength, bLength) = (arrayA.Length + 1, arrayB.Length + 1);
            var dtw = new double[2 * bLength];

#if NET6_0_OR_GREATER
            var spanA = arrayA.AsSpan();
            var spanB = arrayB.AsSpan();
#else
            var spanA = arrayA;
            var spanB = arrayB;
#endif
            int previousRow = 0, currentRow = -bLength;

            for (var i = 1; i < aLength; i++)
            {
                currentRow += bLength;
                if (currentRow == dtw.Length)
                {
                    currentRow = 0;
                }

                for (var j = 1; j < bLength; j++)
                {
                    double lastMin;
                    if (i == 1 && j == 1)
                    {
                        lastMin = dtw[previousRow + j - 1];
                    }
                    else if (i == 1)
                    {
                        lastMin = dtw[currentRow + j - 1];
                    }
                    else if (j == 1)
                    {
                        lastMin = dtw[previousRow + j];
                    }
                    else
                    {
                        lastMin = Math.Min(dtw[previousRow + j],
                            Math.Min(dtw[currentRow + j - 1], dtw[previousRow + j - 1]));
                    }

                    dtw[currentRow + j] = Math.Abs(spanA[i - 1] - spanB[j - 1]) + lastMin;
                }

                previousRow = currentRow;
            }

            return dtw[currentRow + bLength - 1];
        }

        public static float GetScoreF(float[] arrayA, float[] arrayB)
        {
            var (aLength, bLength) = (arrayA.Length + 1, arrayB.Length + 1);
            var dtw = new float[2 * bLength];

#if NET6_0_OR_GREATER
            var spanA = arrayA.AsSpan();
            var spanB = arrayB.AsSpan();
#else
            var spanA = arrayA;
            var spanB = arrayB;
#endif

            int previousRow = 0, currentRow = -bLength;

            for (var i = 1; i < aLength; i++)
            {
                currentRow += bLength;
                if (currentRow == dtw.Length)
                {
                    currentRow = 0;
                }

                for (var j = 1; j < bLength; j++)
                {
                    float lastMin;
                    if (i == 1 && j == 1)
                    {
                        lastMin = dtw[previousRow + j - 1];
                    }
                    else if (i == 1)
                    {
                        lastMin = dtw[currentRow + j - 1];
                    }
                    else if (j == 1)
                    {
                        lastMin = dtw[previousRow + j];
                    }
                    else
#if NET6_0_OR_GREATER
                        lastMin = MathF.Min(dtw[previousRow + j],
                            MathF.Min(dtw[currentRow + j - 1], dtw[previousRow + j - 1]));

                    dtw[currentRow + j] = MathF.Abs(spanA[i - 1] - spanB[j - 1]) + lastMin;
#else
                    {
                        lastMin = Math.Min(dtw[previousRow + j],
                            Math.Min(dtw[currentRow + j - 1], dtw[previousRow + j - 1]));
                    }

                    dtw[currentRow + j] = Math.Abs(spanA[i - 1] - spanB[j - 1]) + lastMin;
#endif
                }

                previousRow = currentRow;
            }

            return dtw[currentRow + bLength - 1];
        }
    }
}