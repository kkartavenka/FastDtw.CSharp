using System;
using System.Linq;
using System.Reflection;
using FastDtw.CSharp;
using FastDtw.CSharp.Implementations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FastDtw.CSharp.NetStandard.Test;

/// <summary>
/// Exercises the library's netstandard2.0 asset, which differs from the net6.0 one by more than
/// compilation flags: GetScore routes through the unsafe pointer kernels rather than the SIMD ones,
/// the Span overloads are absent, and the batch API does not exist. The banded, LB_Keogh and path
/// code is unguarded and so ships to netstandard2.0 too — that is what these tests cover.
/// </summary>
[TestClass]
public class NetStandardBuildTests
{
    private static readonly Assembly Library = typeof(Dtw).Assembly;

    /// <summary>
    /// Guards the premise of every other test here. If the project reference ever resolves to the
    /// net6.0 asset instead, this fails loudly rather than silently retesting net6.0.
    /// </summary>
    [TestMethod]
    public void AssemblyIsNetStandardBuild()
    {
        var target = Library.GetCustomAttribute<System.Runtime.Versioning.TargetFrameworkAttribute>();
        Assert.IsNotNull(target, "library has no TargetFrameworkAttribute");
        StringAssert.Contains(target!.FrameworkName, ".NETStandard,Version=v2.0",
            $"loaded the wrong asset: {target.FrameworkName}");

        // Compiled out under NETSTANDARD2_0 — their absence confirms the guards took effect.
        Assert.IsNull(Library.GetType("FastDtw.CSharp.Implementations.UnweightedDtwSimd"),
            "UnweightedDtwSimd should not exist in the netstandard2.0 build");
        Assert.IsNull(Library.GetType("FastDtw.CSharp.Implementations.DtwBatch"),
            "DtwBatch should not exist in the netstandard2.0 build");

        var spanOverloads = typeof(Dtw).GetMethods()
            .Where(m => m.Name == nameof(Dtw.GetScore))
            .Count(m => m.GetParameters().Any(p => p.ParameterType.Name.StartsWith("Span")));
        Assert.AreEqual(0, spanOverloads, "Span overloads should not exist in the netstandard2.0 build");

        // Unguarded, so these must be present for netstandard2.0 consumers.
        Assert.IsNotNull(Library.GetType("FastDtw.CSharp.Implementations.UnweightedDtwBanded"));
        Assert.IsNotNull(Library.GetType("FastDtw.CSharp.Implementations.LbKeogh"));
    }

    private static double[] Random(Random rng, int n)
    {
        var a = new double[n];
        for (var i = 0; i < n; i++) a[i] = rng.NextDouble() * 200 - 100;
        return a;
    }

    /// <summary>The unsafe kernel must produce the same documented values as the net6.0 build.</summary>
    [TestMethod]
    public void UnsafeKernelMatchesKnownValues()
    {
        var a = new double[] { 41.98, 41.65, 42.01, 42.35, 44.4, 43.08, 43.6, 42.84, 42.83, 44.01, 43.07, 44.3, 44.6, 46.54, 45.06, 44.96, 43.59, 46.84, 45.22, 45.52 };
        var b = new double[] { 95.07, 93.5, 96.67, 96.28, 102.47, 94.24, 95.12, 87.06, 87.92, 88.73, 86.36, 95.34, 93.87, 99.42, 91.13, 89.84, 85.52, 89.64, 84.91, 85.16 };
        Assert.AreEqual(959.79999999999984, Dtw.GetScore(a, b));

        var shorter = new double[] { 95.07, 93.5, 96.67, 96.28, 102.47, 94.24, 95.12, 87.06, 87.92, 88.73, 86.36, 95.34, 93.87, 99.42 };
        Assert.AreEqual(951.76, Dtw.GetScore(a, shorter));

        var af = Array.ConvertAll(a, x => (float)x);
        var bf = Array.ConvertAll(b, x => (float)x);
        Assert.AreEqual(959.800048828125f, Dtw.GetScore(af, bf));
    }

    [TestMethod]
    public void BandedIsAvailableAndConsistent()
    {
        var rng = new Random(31337);
        for (var t = 0; t < 40; t++)
        {
            int n = rng.Next(2, 70), m = rng.Next(2, 70);
            double[] a = Random(rng, n), b = Random(rng, m);

            // A band at least as wide as the series admits every path.
            Assert.AreEqual(Dtw.GetScore(a, b), Dtw.GetScore(a, b, Math.Max(n, m)), 1e-9, $"{n}x{m}");

            // Widening can only admit cheaper paths, and never goes below the unconstrained score.
            var exact = Dtw.GetScore(a, b);
            var previous = double.PositiveInfinity;
            for (var w = 0; w <= Math.Max(n, m); w += 5)
            {
                var score = Dtw.GetScore(a, b, w);
                Assert.IsTrue(score >= exact - 1e-9, $"band {w} fell below unconstrained");
                Assert.IsTrue(score <= previous + 1e-9, $"band {w} exceeded a narrower band");
                previous = score;
            }
        }
    }

    [TestMethod]
    public void ZeroBandOnEqualLengthsIsPointwiseL1()
    {
        var a = new double[] { 1, 5, 2, 8, 3 };
        var b = new double[] { 4, 4, 9, 1, 3 };
        Assert.AreEqual(3d + 1d + 7d + 7d + 0d, Dtw.GetScore(a, b, 0), 1e-9);
    }

    [TestMethod]
    public void LowerBoundIsSoundAndSearchMatchesBruteForce()
    {
        var rng = new Random(8080);
        for (var t = 0; t < 60; t++)
        {
            var n = rng.Next(2, 60);
            var band = rng.Next(0, n);
            var count = rng.Next(1, 15);

            var query = Random(rng, n);
            var subjects = new double[count][];
            for (var i = 0; i < count; i++) subjects[i] = Random(rng, n);

            foreach (var subject in subjects)
            {
                Assert.IsTrue(Dtw.GetLowerBound(query, subject, band) <= Dtw.GetScore(query, subject, band) + 1e-9,
                    "LB_Keogh exceeded the banded distance it bounds");
            }

            var expectedDistance = double.PositiveInfinity;
            var expectedIndex = -1;
            for (var i = 0; i < count; i++)
            {
                var d = Dtw.GetScore(query, subjects[i], band);
                if (d < expectedDistance) { expectedDistance = d; expectedIndex = i; }
            }

            var index = Dtw.GetNearestNeighbor(query, subjects, band, out var distance);
            Assert.AreEqual(expectedIndex, index, $"n={n}, count={count}, band={band}");
            Assert.AreEqual(expectedDistance, distance, 1e-9);
        }
    }

    /// <summary>The rewritten GetPath ships to netstandard2.0 too; diff it against the dense original.</summary>
    [TestMethod]
    public void PathMatchesDenseImplementation()
    {
        var rng = new Random(606);
        for (var t = 0; t < 80; t++)
        {
            int n = rng.Next(2, 90), m = rng.Next(2, 90);
            double[] a = Random(rng, n), b = Random(rng, m);

            var expected = UnweightedDtwPath.GetPathDense(a, b);
            var actual = Dtw.GetPath(a, b);

            Assert.AreEqual(expected.Score, actual.Score, $"score {n}x{m}");
            Assert.AreEqual(expected.Path.Count, actual.Path.Count, $"length {n}x{m}");
            for (var k = 0; k < expected.Path.Count; k++)
            {
                Assert.AreEqual(expected.Path[k].Item1, actual.Path[k].Item1, $"path[{k}].i {n}x{m}");
                Assert.AreEqual(expected.Path[k].Item2, actual.Path[k].Item2, $"path[{k}].j {n}x{m}");
            }
        }
    }

    [TestMethod]
    public void WeightedAndNormalizedScoresStillWork()
    {
        var a = new double[] { 41.98, 41.65, 42.01, 42.35, 44.4 };
        var b = new double[] { 95.07, 93.5, 96.67, 96.28, 102.47 };
        var weights = Enumerable.Repeat(1d, 5).ToArray();

        Assert.AreEqual(Dtw.GetScore(a, b), Dtw.GetWeightedScore(a, b, weights, weights, WeightingApproach.Multiplicative), 1e-9);

        var normalized = Dtw.GetScore(a, b, NormalizationType.MaxSeriesLength);
        Assert.AreEqual(Dtw.GetScore(a, b) / 5d, normalized, 1e-9);
    }

    [TestMethod]
    public void ValidationStillApplies()
    {
        Assert.ThrowsException<ArgumentException>(() => Dtw.GetScore(new double[1], new double[1]));
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => Dtw.GetScore(new double[3], new double[3], -1));
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => Dtw.GetLowerBound(new double[3], new double[3], -1));
        Assert.ThrowsException<ArgumentException>(() => Dtw.GetLowerBound(new double[4], new double[3], 1));
    }
}
