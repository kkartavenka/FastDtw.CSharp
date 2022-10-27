using FastDtw.CSharp;
using System;
using System.IO;

namespace Sample; 
internal class Program {

    private const string _testFile = @".\..\..\..\..\Data\Test.csv";

    static void Main() {
        var data = GetData();

        Console.WriteLine($"DTW score (double): {Dtw.GetScore(data.arrayA, data.arrayB)}");
        Console.WriteLine($"DTW score (float): {Dtw.GetScoreF(data.arrayAF, data.arrayBF)}");
        Console.WriteLine($"DTW score (double) [GPU]: {DtwGpu.GetScore(data.arrayA, data.arrayB)}");
    }

    private static (double[] arrayA, double[] arrayB, float[] arrayAF, float[] arrayBF) GetData() {
        var lines = File.ReadAllLines(_testFile);

        var arrayA = new double[lines.Length];
        var arrayB = new double[lines.Length];
        var arrayAF = new float[lines.Length];
        var arrayBF = new float[lines.Length];

        int idxA = 0, idxB = 0;
        foreach (var line in lines) {
            var splittedString = line.Split(',');

            if (double.TryParse(splittedString[0], out double varA)) {
                arrayAF[idxA] = (float)varA;
                arrayA[idxA++] = varA;
            }

            if (double.TryParse(splittedString[1], out double varB)) {
                arrayBF[idxB] = (float)varB;
                arrayB[idxB++] = varB;
            }
        }

        if (idxA != arrayA.Length) {
            arrayA = arrayA[0..idxA];
            arrayAF = arrayAF[0..idxA];
        }

        if (idxB != arrayB.Length) {
            arrayB = arrayB[0..idxB];
            arrayBF = arrayBF[0..idxB];
        }

        return (arrayA[0..idxA], arrayB[0..idxB], arrayAF[0..idxA], arrayBF[0..idxB]);
    }

}