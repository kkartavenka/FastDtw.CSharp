using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FastDtw.CSharp.Test;

/// <summary>
/// The array overloads now run the skewed SIMD kernel while the Span overloads still run the scalar
/// one, which makes the Span overload a ready-made differential oracle. The two must agree
/// bit-for-bit: the optimisation reorders memory access, not arithmetic.
/// </summary>
[TestClass]
public class SimdKernelTests
{
    private static (double[] a, double[] b) Pair(Random rng, int n, int m)
    {
        var a = new double[n];
        var b = new double[m];
        for (var i = 0; i < n; i++) a[i] = rng.NextDouble() * 200 - 100;
        for (var i = 0; i < m; i++) b[i] = rng.NextDouble() * 200 - 100;
        return (a, b);
    }

    /// <summary>Lengths bracketing the SIMD width on any platform, plus awkward remainders.</summary>
    private static readonly int[] EdgeLengths = { 2, 3, 4, 5, 6, 7, 8, 9, 15, 16, 17, 31, 32, 33 };

    [TestMethod]
    public void SimdMatchesScalarOnEdgeLengths()
    {
        var rng = new Random(20240521);
        foreach (var n in EdgeLengths)
        {
            foreach (var m in EdgeLengths)
            {
                var (a, b) = Pair(rng, n, m);
                Assert.AreEqual(Dtw.GetScore(a.AsSpan(), b.AsSpan()), Dtw.GetScore(a, b), $"double {n}x{m}");

                var af = Array.ConvertAll(a, x => (float)x);
                var bf = Array.ConvertAll(b, x => (float)x);
                Assert.AreEqual(Dtw.GetScore(af.AsSpan(), bf.AsSpan()), Dtw.GetScore(af, bf), $"float {n}x{m}");
            }
        }
    }

    [TestMethod]
    public void SimdMatchesScalarOnRandomShapes()
    {
        var rng = new Random(987654);
        for (var t = 0; t < 300; t++)
        {
            int n = rng.Next(2, 200), m = rng.Next(2, 200);
            var (a, b) = Pair(rng, n, m);
            Assert.AreEqual(Dtw.GetScore(a.AsSpan(), b.AsSpan()), Dtw.GetScore(a, b), $"double {n}x{m}");

            var af = Array.ConvertAll(a, x => (float)x);
            var bf = Array.ConvertAll(b, x => (float)x);
            Assert.AreEqual(Dtw.GetScore(af.AsSpan(), bf.AsSpan()), Dtw.GetScore(af, bf), $"float {n}x{m}");
        }
    }

    [TestMethod]
    public void SimdIsSymmetricAndZeroOnIdenticalSeries()
    {
        var rng = new Random(4242);
        for (var t = 0; t < 40; t++)
        {
            var (a, b) = Pair(rng, rng.Next(2, 60), rng.Next(2, 60));
            Assert.AreEqual(Dtw.GetScore(a, b), Dtw.GetScore(b, a), GlobalConstants.DoubleTolerance);
            Assert.AreEqual(0d, Dtw.GetScore(a, a), GlobalConstants.DoubleTolerance);
        }
    }
}
