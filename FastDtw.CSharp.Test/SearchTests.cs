using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FastDtw.CSharp.Test;

/// <summary>
/// LB_Keogh and the pruned nearest-neighbour search. No external oracle exists, so the contract is
/// checked directly: the bound must never exceed the banded distance it bounds, and the pruned
/// search must return exactly what brute force returns.
/// </summary>
[TestClass]
public class SearchTests
{
    private static double[] Random(Random rng, int n)
    {
        var a = new double[n];
        for (var i = 0; i < n; i++) a[i] = rng.NextDouble() * 200 - 100;
        return a;
    }

    /// <summary>Soundness: LB_Keogh must never exceed the banded distance for the same band.</summary>
    [TestMethod]
    public void LowerBoundNeverExceedsBandedDistance()
    {
        var rng = new Random(8080);
        for (var t = 0; t < 400; t++)
        {
            var n = rng.Next(2, 80);
            double[] q = Random(rng, n), c = Random(rng, n);
            var band = rng.Next(0, n);

            var bound = Dtw.GetLowerBound(q, c, band);
            var actual = Dtw.GetScore(q, c, band);

            Assert.IsTrue(bound <= actual + 1e-9, $"LB {bound} exceeded banded DTW {actual} (n={n}, band={band})");
        }
    }

    [TestMethod]
    public void LowerBoundNeverExceedsBandedDistanceF()
    {
        var rng = new Random(9191);
        for (var t = 0; t < 400; t++)
        {
            var n = rng.Next(2, 80);
            var q = Array.ConvertAll(Random(rng, n), x => (float)x);
            var c = Array.ConvertAll(Random(rng, n), x => (float)x);
            var band = rng.Next(0, n);

            var bound = Dtw.GetLowerBound(q, c, band);
            var actual = Dtw.GetScore(q, c, band);

            Assert.IsTrue(bound <= actual + 1e-3f, $"LB {bound} exceeded banded DTW {actual} (n={n}, band={band})");
        }
    }

    /// <summary>Exactness: pruning may only remove candidates that could not have won.</summary>
    [TestMethod]
    public void PrunedSearchMatchesBruteForce()
    {
        var rng = new Random(606060);
        for (var t = 0; t < 60; t++)
        {
            int n = rng.Next(5, 50), count = rng.Next(1, 25);
            var query = Random(rng, n);
            var subjects = new double[count][];
            for (var i = 0; i < count; i++) subjects[i] = Random(rng, n);
            var band = rng.Next(0, n);

            // Brute force with the same tie-breaking: first strict improvement wins.
            var expectedDistance = double.PositiveInfinity;
            var expectedIndex = -1;
            for (var i = 0; i < count; i++)
            {
                var d = Dtw.GetScore(query, subjects[i], band);
                if (d < expectedDistance)
                {
                    expectedDistance = d;
                    expectedIndex = i;
                }
            }

            var index = Dtw.GetNearestNeighbor(query, subjects, band, out var distance);
            Assert.AreEqual(expectedIndex, index, $"index mismatch (n={n}, count={count}, band={band})");
            Assert.AreEqual(expectedDistance, distance, 1e-9, "distance mismatch");
        }
    }

    [TestMethod]
    public void QueryFindsItselfAtZeroDistance()
    {
        var rng = new Random(4321);
        var query = Random(rng, 40);
        var subjects = new[] { Random(rng, 40), Random(rng, 40), query, Random(rng, 40) };

        Assert.AreEqual(2, Dtw.GetNearestNeighbor(query, subjects, 5, out var distance));
        Assert.AreEqual(0d, distance, GlobalConstants.DoubleTolerance);
    }

    /// <summary>
    /// The bound must actually prune, or the exactness tests say nothing about its usefulness.
    /// Measured on the real series: 38 of 40 candidates are skipped.
    /// </summary>
    [TestMethod]
    public void BoundPrunesMostCandidatesOnRealData()
    {
        var series = TestSeries.LoadColumnA();
        const int length = 128, band = 8, count = 40;

        var query = series[1000..(1000 + length)];
        var subjects = new double[count][];
        for (var i = 0; i < count; i++)
        {
            var start = 200 + i * 137;
            subjects[i] = series[start..(start + length)];
        }

        var best = double.PositiveInfinity;
        foreach (var subject in subjects) best = Math.Min(best, Dtw.GetScore(query, subject, band));

        var prunable = 0;
        foreach (var subject in subjects)
        {
            if (Dtw.GetLowerBound(query, subject, band) >= best) prunable++;
        }

        Assert.IsTrue(prunable >= 30, $"only {prunable}/{count} candidates were prunable — the bound has regressed");
    }

    /// <summary>
    /// Characterisation, not a defect: on white noise the envelope spans nearly the whole value
    /// range, so the bound collapses toward zero and prunes almost nothing. It stays sound, but buys
    /// no speed. Documented so a future change that appears to "fix" this is recognised as unsound.
    /// </summary>
    [TestMethod]
    public void BoundIsWeakOnWhiteNoise()
    {
        var rng = new Random(555111);
        const int length = 60, band = 5, count = 40;

        var query = Random(rng, length);
        var subjects = new double[count][];
        for (var i = 0; i < count; i++)
        {
            subjects[i] = Random(rng, length);
            for (var k = 0; k < length; k++) subjects[i][k] += i * 3d;
        }

        var best = double.PositiveInfinity;
        foreach (var subject in subjects) best = Math.Min(best, Dtw.GetScore(query, subject, band));

        var prunable = 0;
        foreach (var subject in subjects)
        {
            var bound = Dtw.GetLowerBound(query, subject, band);
            Assert.IsTrue(bound <= Dtw.GetScore(query, subject, band) + 1e-9, "bound must stay sound even when loose");
            if (bound >= best) prunable++;
        }

        Assert.IsTrue(prunable < count / 2, $"{prunable}/{count} prunable on white noise — verify the envelope");
    }

    [TestMethod]
    public void RejectsUnequalLengthsAndBadArguments()
    {
        var q = new double[] { 1, 2, 3, 4 };
        var shorter = new double[] { 1, 2, 3 };

        Assert.ThrowsException<ArgumentException>(() => Dtw.GetLowerBound(q, shorter, 1));
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => Dtw.GetLowerBound(q, q, -1));
        Assert.ThrowsException<ArgumentException>(() => Dtw.GetNearestNeighbor(q, new[] { shorter }, 1, out _));
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => Dtw.GetNearestNeighbor(q, new[] { q }, -1, out _));
    }

    [TestMethod]
    public void EmptySubjectSetReturnsNoMatch()
    {
        var q = new double[] { 1, 2, 3 };
        Assert.AreEqual(-1, Dtw.GetNearestNeighbor(q, Array.Empty<double[]>(), 1, out var distance));
        Assert.IsTrue(double.IsPositiveInfinity(distance));
    }
}
