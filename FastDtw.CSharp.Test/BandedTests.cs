using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FastDtw.CSharp.Test;

/// <summary>
/// Banded DTW has no external oracle: the FastDtw package's radius is a multiresolution
/// approximation parameter, not a Sakoe-Chiba band, so the results are not comparable. These are
/// self-consistency properties plus hand-checked cases.
/// </summary>
[TestClass]
public class BandedTests
{
    private static double[] Random(Random rng, int n)
    {
        var a = new double[n];
        for (var i = 0; i < n; i++) a[i] = rng.NextDouble() * 200 - 100;
        return a;
    }

    /// <summary>A band at least as wide as the series admits every path, so it equals exact DTW.</summary>
    [TestMethod]
    public void WideBandEqualsUnconstrained()
    {
        var rng = new Random(31337);
        for (var t = 0; t < 60; t++)
        {
            int n = rng.Next(2, 80), m = rng.Next(2, 80);
            double[] a = Random(rng, n), b = Random(rng, m);
            var wide = Math.Max(n, m);

            Assert.AreEqual(Dtw.GetScore(a, b), Dtw.GetScore(a, b, wide), GlobalConstants.DoubleTolerance, $"double {n}x{m}");

            var af = Array.ConvertAll(a, x => (float)x);
            var bf = Array.ConvertAll(b, x => (float)x);
            Assert.AreEqual(Dtw.GetScore(af, bf), Dtw.GetScore(af, bf, wide), GlobalConstants.FloatTolerance, $"float {n}x{m}");
        }
    }

    /// <summary>Relaxing the constraint can only ever admit cheaper paths.</summary>
    [TestMethod]
    public void WideningBandIsMonotonicNonIncreasing()
    {
        var rng = new Random(24680);
        for (var t = 0; t < 40; t++)
        {
            int n = rng.Next(20, 90), m = rng.Next(20, 90);
            double[] a = Random(rng, n), b = Random(rng, m);

            var previous = double.PositiveInfinity;
            for (var w = 0; w <= Math.Max(n, m); w += 3)
            {
                var score = Dtw.GetScore(a, b, w);
                Assert.IsTrue(score <= previous + 1e-9, $"band {w} scored {score}, above the narrower band's {previous}");
                previous = score;
            }

            Assert.AreEqual(Dtw.GetScore(a, b), previous, 1e-9);
        }
    }

    /// <summary>A banded result is an upper bound on the unconstrained one, never below it.</summary>
    [TestMethod]
    public void BandedIsNeverBelowUnconstrained()
    {
        var rng = new Random(13579);
        for (var t = 0; t < 60; t++)
        {
            int n = rng.Next(5, 70), m = rng.Next(5, 70);
            double[] a = Random(rng, n), b = Random(rng, m);
            var exact = Dtw.GetScore(a, b);

            for (var w = 0; w < 12; w++)
            {
                Assert.IsTrue(Dtw.GetScore(a, b, w) >= exact - 1e-9, $"band {w} fell below the unconstrained score");
            }
        }
    }

    /// <summary>Identical series warp along the diagonal, which every band admits.</summary>
    [TestMethod]
    public void IdenticalSeriesScoreZeroAtEveryBand()
    {
        var a = new double[] { 1, 5, 2, 8, 3, 9, 4 };
        for (var w = 0; w <= a.Length; w++)
        {
            Assert.AreEqual(0d, Dtw.GetScore(a, a, w), GlobalConstants.DoubleTolerance, $"band {w}");
        }
    }

    /// <summary>Band 0 on equal lengths forces the diagonal, so the result is pointwise L1.</summary>
    [TestMethod]
    public void ZeroBandOnEqualLengthsIsPointwiseL1()
    {
        var a = new double[] { 1, 5, 2, 8, 3 };
        var b = new double[] { 4, 4, 9, 1, 3 };
        const double expected = 3d + 1d + 7d + 7d + 0d;

        Assert.AreEqual(expected, Dtw.GetScore(a, b, 0), 1e-9);
    }

    /// <summary>The band widens by the length difference, so unequal lengths stay reachable.</summary>
    [TestMethod]
    public void UnequalLengthsRemainFeasibleAtZeroBand()
    {
        var a = new double[] { 1, 2, 3, 4, 5, 6, 7, 8 };
        var b = new double[] { 1, 2, 3 };

        var score = Dtw.GetScore(a, b, 0);
        Assert.IsFalse(double.IsInfinity(score), "no path was admitted");
        Assert.IsTrue(score >= Dtw.GetScore(a, b) - 1e-9);
    }

    [TestMethod]
    public void RejectsNegativeBand()
    {
        var a = new double[] { 1, 2, 3 };
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => Dtw.GetScore(a, a, -1));
        var af = new float[] { 1, 2, 3 };
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => Dtw.GetScore(af, af, -1));
    }
}
