using BenchmarkDotNet.Attributes;

namespace Benchmark;

[MemoryDiagnoser]
public class BenchmarkDemo {
    // Need to find how to specify relative path with BenchmarkDotNet
    private const string _testFile = @"C:\Users\kkart\source\repos\FastDtw.CSharp\Data\Test.csv";
    private readonly double[] _arrayA, _arrayB;
    private readonly float[] _arrayAF, _arrayBF;
    public BenchmarkDemo() => (_arrayA, _arrayB, _arrayAF, _arrayBF) = GetData();

    [Params(0, 10, 500)]
    public int BenchmarkSequenceLength { get; set; }

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

    [Benchmark(Baseline = true, Description = "FastDtw")]
    public double FastDtwRun() {
        if (BenchmarkSequenceLength == 0)
            return FastDtw.Dtw.Distance(_arrayA, _arrayB);
        else
            return FastDtw.Dtw.Distance(_arrayA[0..BenchmarkSequenceLength], _arrayB[0..BenchmarkSequenceLength]);
    }

    [Benchmark (Description = "NDtw")]
    public double NDtwRun() {
        if (BenchmarkSequenceLength == 0)
            return new NDtw.Dtw(_arrayA, _arrayB).GetCost();
        else
            return new NDtw.Dtw(_arrayA[0..BenchmarkSequenceLength], _arrayB[0..BenchmarkSequenceLength]).GetCost();

    }

    [Benchmark(Description = "FastDtw.CSharp (double)")]
    public double FastDtwCSharpRun() {
        if (BenchmarkSequenceLength == 0)
            return FastDtw.CSharp.Dtw.GetScore(_arrayA, _arrayB);
        else
            return FastDtw.CSharp.Dtw.GetScore(_arrayA[0..BenchmarkSequenceLength], _arrayB[0..BenchmarkSequenceLength]);
    }

    [Benchmark (Description = "FastDtw.CSharp (float)")]
    public float FastDtwCSharpRunF() {
        if (BenchmarkSequenceLength == 0)
            return FastDtw.CSharp.Dtw.GetScoreF(_arrayAF, _arrayBF);
        else
            return FastDtw.CSharp.Dtw.GetScoreF(_arrayAF[0..BenchmarkSequenceLength], _arrayBF[0..BenchmarkSequenceLength]);
    }

    [Benchmark(Description = "ADN.TimeSeries")]
    public double ADNDtwRun() {
        if (BenchmarkSequenceLength == 0)
            return new ADN.TimeSeries.DTW(_arrayA, _arrayB).GetSum();
        else
            return new ADN.TimeSeries.DTW(_arrayA[0..BenchmarkSequenceLength], _arrayB[0..BenchmarkSequenceLength]).GetSum();
    }

}
