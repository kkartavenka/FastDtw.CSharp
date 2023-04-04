using System;

namespace FastDtw.CSharp;
public static class Dtw {

    private static double GetScoreSlower(double[] arrayA, double[] arrayB) {
        var (aLength, bLength) = (arrayA.Length + 1, arrayB.Length + 1);

        var spanA = arrayA.AsSpan();
        var spanB = arrayB.AsSpan();

        double[][] dtw = new double[aLength][];
        dtw[0] = new double[bLength];

        double[] currentDtwRow;
        var previousDtwRow = dtw[0];

        for (var i = 1; i < aLength; i++) {
            currentDtwRow = new double[bLength];

            for (var j = 1; j < bLength; j++) {
                var cost = Math.Abs(spanA[i - 1] - spanB[j - 1]);

                double lastMin;
                if (i == 1 && j == 1)
                    lastMin = previousDtwRow[j - 1];
                else if (i == 1)
                    lastMin = currentDtwRow[j - 1];
                else if (j == 1)
                    lastMin = previousDtwRow[j];
                else
                    lastMin = Math.Min(previousDtwRow[j], Math.Min(currentDtwRow[j - 1], previousDtwRow[j - 1]));

                currentDtwRow[j] = cost + lastMin;
            }
            dtw[i] = currentDtwRow;
            previousDtwRow = currentDtwRow;
        }

        return dtw[aLength - 1][bLength - 1];
    }

    public static double GetScore(double[] arrayA, double[] arrayB)
    {
        var (aLength, bLength) = (arrayA.Length + 1, arrayB.Length + 1);
        var dtw = new double[2 * bLength];

        var spanA = arrayA.AsSpan();
        var spanB = arrayB.AsSpan();

        int previousRow = 0, currentRow = -bLength;

        for (var i = 1; i < aLength; i++)
        {
            currentRow += bLength;
            if (currentRow == dtw.Length)
                currentRow = 0;

            for (var j = 1; j < bLength; j++)
            {
                double lastMin;
                if (i == 1 && j == 1)
                    lastMin = dtw[previousRow + j - 1];
                else if (i == 1)
                    lastMin = dtw[currentRow + j - 1];
                else if (j == 1)
                    lastMin = dtw[previousRow + j];
                else
                    lastMin = Math.Min(dtw[previousRow + j], Math.Min(dtw[currentRow + j - 1], dtw[previousRow + j - 1]));

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

        var spanA = arrayA.AsSpan();
        var spanB = arrayB.AsSpan();

        int previousRow = 0, currentRow = -bLength;

        for (var i = 1; i < aLength; i++)
        {
            currentRow += bLength;
            if (currentRow == dtw.Length)
                currentRow = 0;

            for (var j = 1; j < bLength; j++)
            {
                float lastMin;
                if (i == 1 && j == 1)
                    lastMin = dtw[previousRow + j - 1];
                else if (i == 1)
                    lastMin = dtw[currentRow + j - 1];
                else if (j == 1)
                    lastMin = dtw[previousRow + j];
                else
                    lastMin = MathF.Min(dtw[previousRow + j], MathF.Min(dtw[currentRow + j - 1], dtw[previousRow + j - 1]));

                dtw[currentRow + j] = MathF.Abs(spanA[i - 1] - spanB[j - 1]) + lastMin;
            }

            previousRow = currentRow;
        }

        return dtw[currentRow + bLength - 1];
    }

    private static float GetScoreFSlower(float[] arrayA, float[] arrayB) {
        var (aLength, bLength) = (arrayA.Length + 1, arrayB.Length + 1);

        var spanA = arrayA.AsSpan();
        var spanB = arrayB.AsSpan();

        float[][] dtw = new float[aLength][];
        dtw[0] = new float[bLength];

        float[] currentDtwRow;
        var previousDtwRow = dtw[0];

        for (var i = 1; i < aLength; i++) {
            currentDtwRow = new float[bLength];

            for (var j = 1; j < bLength; j++) {
                var cost = MathF.Abs(spanA[i - 1] - spanB[j - 1]);

                float lastMin;
                if (i == 1 && j == 1)
                    lastMin = previousDtwRow[j - 1];
                else if (i == 1)
                    lastMin = currentDtwRow[j - 1];
                else if (j == 1)
                    lastMin = previousDtwRow[j];
                else
                    lastMin = MathF.Min(previousDtwRow[j], MathF.Min(currentDtwRow[j - 1], previousDtwRow[j - 1]));

                currentDtwRow[j] = cost + lastMin;
            }
            dtw[i] = currentDtwRow;
            previousDtwRow = currentDtwRow;
        }

        return dtw[aLength - 1][bLength - 1];
    }

}
