using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using FastDtw.CSharp;
using Microsoft.FSharp.Collections;

namespace Benchmark;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net481)]
[SimpleJob(RuntimeMoniker.Net60)]
[SimpleJob(RuntimeMoniker.Net80)]
public class BenchmarkDemo {
    private const string TestFile = @"Test.csv";
    private double[] _arrayA, _arrayB;
    private float[] _fArrayA, _fArrayB;

    private static (double[] arrayA, double[] arrayB, float[] arrayAF, float[] arrayBF) GetData()
    {
        var lines = File.ReadAllLines(TestFile);

        var arrayA = new double[lines.Length];
        var arrayB = new double[lines.Length];
        var fArrayA = new float[lines.Length];
        var fArrayB = new float[lines.Length];

        int idxA = 0, idxB = 0;
        foreach (var line in lines)
        {
            var splittedString = line.Split(',');

            if (double.TryParse(splittedString[0], out double varA))
            {
                fArrayA[idxA] = (float)varA;
                arrayA[idxA++] = varA;
            }

            if (double.TryParse(splittedString[1], out double varB))
            {
                fArrayB[idxB] = (float)varB;
                arrayB[idxB++] = varB;
            }
        }

        if (idxA != arrayA.Length)
        {
            arrayA = arrayA[0..idxA];
            fArrayA = fArrayA[0..idxA];
        }

        if (idxB != arrayB.Length)
        {
            arrayB = arrayB[0..idxB];
            fArrayB = fArrayB[0..idxB];
        }

        return (arrayA[0..idxA], arrayB[0..idxB], fArrayA[0..idxA], fArrayB[0..idxB]);
    }
    
    [Params(0, 10, 500)]
    public int BenchmarkSequenceLength { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        (_arrayA, _arrayB, _fArrayA, _fArrayB) = GetData();
    }

    [Benchmark(Description = "FastDtw.Distance()")]
    public double FastDtwRun()
    {
        return BenchmarkSequenceLength == 0 
            ? FastDtw.Dtw.Distance(_arrayA, _arrayB) 
            : FastDtw.Dtw.Distance(_arrayA[0..BenchmarkSequenceLength], _arrayB[0..BenchmarkSequenceLength]);
    }
    
    [Benchmark(Description = "FastDtw.DistanceWithPath()")]
    public Tuple<double, FSharpList<Tuple<int,int>>> FastDtwWithPathRun()
    {
        var radius = Math.Max(_arrayA.Length, _arrayB.Length);
        return BenchmarkSequenceLength == 0 
            ? FastDtw.Dtw.DistanceWithPath(_arrayA, _arrayB, radius) 
            : FastDtw.Dtw.DistanceWithPath(_arrayA[0..BenchmarkSequenceLength], _arrayB[0..BenchmarkSequenceLength], radius);
    }

    [Benchmark (Description = "NDtw")]
    public double NDtwRun()
    {
        return BenchmarkSequenceLength == 0 
            ? new NDtw.Dtw(_arrayA, _arrayB).GetCost() 
            : new NDtw.Dtw(_arrayA[0..BenchmarkSequenceLength], _arrayB[0..BenchmarkSequenceLength]).GetCost();
    }
    
    [Benchmark(Description = "FastDtw.CSharp.GetScore()", Baseline = true)]
    public double FastDtwCSharpGetScoreRun()
    {
        return BenchmarkSequenceLength == 0 
            ? Dtw.GetScore(_arrayA, _arrayB) 
            : Dtw.GetScore(_arrayA[0..BenchmarkSequenceLength], _arrayB[0..BenchmarkSequenceLength]);
    }
    
    [Benchmark(Description = "FastDtw.CSharp.GetScoreF()")]
    public double FastDtwCSharpGetScoreRunF()
    {
        return BenchmarkSequenceLength == 0 
            ? Dtw.GetScoreF(_fArrayA, _fArrayB) 
            : Dtw.GetScoreF(_fArrayA[0..BenchmarkSequenceLength], _fArrayB[0..BenchmarkSequenceLength]);
    }
    
    [Benchmark(Description = "FastDtw.CSharp.GetPath")]
    public PathResult FastDtwCSharpRunPath()
    {
        return BenchmarkSequenceLength == 0 
            ? Dtw.GetPath(_arrayA, _arrayB) 
            : Dtw.GetPath(_arrayA[0..BenchmarkSequenceLength], _arrayB[0..BenchmarkSequenceLength]);
    }

    [Benchmark(Description = "ADN.TimeSeries")]
    public double ADNDtwRun()
    {
        return BenchmarkSequenceLength == 0 
            ? new ADN.TimeSeries.DTW(_arrayA, _arrayB).GetSum() 
            : new ADN.TimeSeries.DTW(_arrayA[0..BenchmarkSequenceLength], _arrayB[0..BenchmarkSequenceLength]).GetSum();
    }

}
