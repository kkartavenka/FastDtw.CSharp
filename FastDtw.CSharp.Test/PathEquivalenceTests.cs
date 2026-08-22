using System;
using FastDtw.CSharp.Implementations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FastDtw.CSharp.Test;

/// <summary>
/// GetPath no longer keeps the full double[n, m] cost matrix — it records one byte per cell and two
/// rolling rows instead. UnweightedDtwPath.GetPathDense is the original dense implementation, kept
/// as an oracle: the two must return the same score and the same path, cell for cell.
/// </summary>
[TestClass]
public class PathEquivalenceTests
{
    private static (double[] a, double[] b) Pair(Random rng, int n, int m)
    {
        var a = new double[n];
        var b = new double[m];
        for (var i = 0; i < n; i++) a[i] = rng.NextDouble() * 200 - 100;
        for (var i = 0; i < m; i++) b[i] = rng.NextDouble() * 200 - 100;
        return (a, b);
    }

    private static void AssertSamePath(double[] a, double[] b, string context)
    {
        var expected = UnweightedDtwPath.GetPathDense(a, b);
        var actual = Dtw.GetPath(a, b);

        Assert.AreEqual(expected.Score, actual.Score, $"score, {context}");
        Assert.AreEqual(expected.Path.Count, actual.Path.Count, $"path length, {context}");

        for (var k = 0; k < expected.Path.Count; k++)
        {
            Assert.AreEqual(expected.Path[k].Item1, actual.Path[k].Item1, $"path[{k}].i, {context}");
            Assert.AreEqual(expected.Path[k].Item2, actual.Path[k].Item2, $"path[{k}].j, {context}");
        }
    }

    [TestMethod]
    public void MatchesDenseImplementationOnRandomShapes()
    {
        var rng = new Random(606);
        for (var t = 0; t < 200; t++)
        {
            int n = rng.Next(2, 120), m = rng.Next(2, 120);
            var (a, b) = Pair(rng, n, m);
            AssertSamePath(a, b, $"{n}x{m}");
        }
    }

    [TestMethod]
    public void MatchesDenseImplementationOnEdgeShapes()
    {
        var rng = new Random(707);
        foreach (var n in new[] { 2, 3, 4, 8, 17 })
        {
            foreach (var m in new[] { 2, 3, 4, 8, 17 })
            {
                var (a, b) = Pair(rng, n, m);
                AssertSamePath(a, b, $"{n}x{m}");
            }
        }
    }

    /// <summary>Ties are where a backtrace can silently diverge, so force plenty of them.</summary>
    [TestMethod]
    public void MatchesDenseImplementationWhenCostsTie()
    {
        var rng = new Random(808);
        for (var t = 0; t < 120; t++)
        {
            int n = rng.Next(2, 60), m = rng.Next(2, 60);
            var a = new double[n];
            var b = new double[m];
            // A tiny integer alphabet makes equal costs, and therefore ties, very common.
            for (var i = 0; i < n; i++) a[i] = rng.Next(0, 3);
            for (var i = 0; i < m; i++) b[i] = rng.Next(0, 3);
            AssertSamePath(a, b, $"tie-heavy {n}x{m}");
        }
    }

    [TestMethod]
    public void MatchesDenseImplementationOnRealData()
    {
        var series = TestSeries.LoadColumnA();
        foreach (var length in new[] { 64, 256, 1000 })
        {
            AssertSamePath(series[0..length], series[500..(500 + length)], $"real {length}");
        }
    }

    /// <summary>The path must be a contiguous monotone walk from (0,0) to the far corner.</summary>
    [TestMethod]
    public void PathIsContiguousAndMonotone()
    {
        var rng = new Random(909);
        for (var t = 0; t < 60; t++)
        {
            int n = rng.Next(2, 90), m = rng.Next(2, 90);
            var (a, b) = Pair(rng, n, m);
            var path = Dtw.GetPath(a, b).Path;

            Assert.AreEqual(0, path[0].Item1);
            Assert.AreEqual(0, path[0].Item2);
            Assert.AreEqual(n - 1, path[^1].Item1);
            Assert.AreEqual(m - 1, path[^1].Item2);

            for (var k = 1; k < path.Count; k++)
            {
                var di = path[k].Item1 - path[k - 1].Item1;
                var dj = path[k].Item2 - path[k - 1].Item2;
                Assert.IsTrue(di is 0 or 1 && dj is 0 or 1 && di + dj > 0,
                    $"illegal step {di},{dj} at {k} ({n}x{m})");
            }
        }
    }
}
