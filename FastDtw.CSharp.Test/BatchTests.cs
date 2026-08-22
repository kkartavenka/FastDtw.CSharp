using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FastDtw.CSharp.Test;

/// <summary>
/// The batch API must be a pure throughput optimisation: every result identical to calling the
/// single-pair overload in a loop, including for ragged input where SIMD lanes cannot be filled and
/// leftovers fall back to the single-pair kernel from several threads at once.
/// </summary>
[TestClass]
public class BatchTests
{
    private static double[] Random(Random rng, int n)
    {
        var a = new double[n];
        for (var i = 0; i < n; i++) a[i] = rng.NextDouble() * 200 - 100;
        return a;
    }

    private static float[] ToF(double[] a) => Array.ConvertAll(a, x => (float)x);

    [TestMethod]
    public void ScoresMatchSinglePairForEqualLengths()
    {
        var rng = new Random(555);
        // Counts deliberately straddle the SIMD width so partial chunks appear too.
        foreach (var count in new[] { 1, 2, 3, 4, 5, 7, 8, 9, 16, 23 })
        {
            var query = Random(rng, 40);
            var subjects = new double[count][];
            for (var i = 0; i < count; i++) subjects[i] = Random(rng, 35);

            var batch = Dtw.GetScores(query, subjects);
            for (var i = 0; i < count; i++)
            {
                Assert.AreEqual(Dtw.GetScore(query, subjects[i]), batch[i], $"double, count {count}, index {i}");
            }

            var queryF = ToF(query);
            var subjectsF = Array.ConvertAll(subjects, ToF);
            var batchF = Dtw.GetScores(queryF, subjectsF);
            for (var i = 0; i < count; i++)
            {
                Assert.AreEqual(Dtw.GetScore(queryF, subjectsF[i]), batchF[i], $"float, count {count}, index {i}");
            }
        }
    }

    /// <summary>Ragged input: lanes fill only from equal-length groups, leftovers go scalar.</summary>
    [TestMethod]
    public void ScoresMatchSinglePairForRaggedLengths()
    {
        var rng = new Random(9090);
        var query = Random(rng, 50);

        var lengths = new[] { 10, 10, 10, 10, 10, 11, 11, 12, 3, 2, 47, 47, 47, 60, 60, 60, 60, 60, 60, 60 };
        var subjects = new double[lengths.Length][];
        for (var i = 0; i < lengths.Length; i++) subjects[i] = Random(rng, lengths[i]);

        var batch = Dtw.GetScores(query, subjects);
        for (var i = 0; i < lengths.Length; i++)
        {
            Assert.AreEqual(Dtw.GetScore(query, subjects[i]), batch[i], $"double, index {i} (length {lengths[i]})");
        }

        var queryF = ToF(query);
        var subjectsF = Array.ConvertAll(subjects, ToF);
        var batchF = Dtw.GetScores(queryF, subjectsF);
        for (var i = 0; i < lengths.Length; i++)
        {
            Assert.AreEqual(Dtw.GetScore(queryF, subjectsF[i]), batchF[i], $"float, index {i} (length {lengths[i]})");
        }
    }

    /// <summary>Large ragged batch, repeated, so parallel threads genuinely interleave.</summary>
    [TestMethod]
    public void LargeRaggedBatchIsStableAcrossRuns()
    {
        var rng = new Random(24680);
        var query = Random(rng, 48);

        var subjects = new double[300][];
        for (var i = 0; i < subjects.Length; i++) subjects[i] = Random(rng, rng.Next(2, 60));

        var expected = new double[subjects.Length];
        for (var i = 0; i < subjects.Length; i++) expected[i] = Dtw.GetScore(query, subjects[i]);

        for (var attempt = 0; attempt < 10; attempt++)
        {
            var batch = Dtw.GetScores(query, subjects);
            for (var i = 0; i < subjects.Length; i++)
            {
                Assert.AreEqual(expected[i], batch[i], $"attempt {attempt}, index {i}");
            }
        }
    }

    [TestMethod]
    public void MatrixIsSymmetricWithZeroDiagonalAndMatchesSinglePair()
    {
        var rng = new Random(31415);
        var series = new double[11][];
        for (var i = 0; i < series.Length; i++) series[i] = Random(rng, 25);

        var matrix = Dtw.GetScoreMatrix(series);
        for (var i = 0; i < series.Length; i++)
        {
            Assert.AreEqual(0d, matrix[i, i], GlobalConstants.DoubleTolerance, $"diagonal at {i}");
            for (var j = i + 1; j < series.Length; j++)
            {
                Assert.AreEqual(Dtw.GetScore(series[i], series[j]), matrix[i, j], $"[{i},{j}]");
                Assert.AreEqual(matrix[i, j], matrix[j, i], $"asymmetry at [{i},{j}]");
            }
        }
    }

    /// <summary>The subject slice shrinks per row, so the grouping differs on every row.</summary>
    [TestMethod]
    public void RaggedMatrixMatchesSinglePair()
    {
        var rng = new Random(97531);
        var lengths = new[] { 11, 11, 11, 11, 11, 19, 19, 19, 26, 26, 5, 5, 5, 33, 33, 33, 8, 8, 47, 47, 12, 12, 12, 12 };
        var series = new float[lengths.Length][];
        for (var i = 0; i < lengths.Length; i++) series[i] = ToF(Random(rng, lengths[i]));

        var matrix = Dtw.GetScoreMatrix(series);
        for (var i = 0; i < series.Length; i++)
        {
            for (var j = i + 1; j < series.Length; j++)
            {
                Assert.AreEqual(Dtw.GetScore(series[i], series[j]), matrix[i, j], $"[{i},{j}]");
                Assert.AreEqual(matrix[i, j], matrix[j, i], $"asymmetry at [{i},{j}]");
            }
        }
    }

    [TestMethod]
    public void RejectsInvalidBatchInput()
    {
        var query = new double[] { 1, 2, 3 };
        var ok = new[] { new double[] { 1, 2 } };

        Assert.ThrowsException<ArgumentNullException>(() => Dtw.GetScores(null!, ok));
        Assert.ThrowsException<ArgumentNullException>(() => Dtw.GetScores(query, null!));
        Assert.ThrowsException<ArgumentException>(() => Dtw.GetScores(query, new[] { new double[] { 1, 2 }, null! }));
    }

    [TestMethod]
    public void EmptySubjectSetReturnsEmptyResults()
    {
        var query = new double[] { 1, 2, 3 };
        Assert.AreEqual(0, Dtw.GetScores(query, Array.Empty<double[]>()).Length);
        Assert.AreEqual(0, Dtw.GetScoreMatrix(Array.Empty<double[]>()).GetLength(0));
    }
}
