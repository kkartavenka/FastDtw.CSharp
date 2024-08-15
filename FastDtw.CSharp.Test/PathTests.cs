using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FastDtw.CSharp.Test;

[TestClass]
public class PathTests
{
    [TestMethod]
    public void GetPath()
    {
        var a = new[] { 1.2, 1.1, 2.1, 2.3, 4.4, 3.0, 3.6, 2.8 };
        var b = new[] { 9.7, 9.5, 9.6, 9.2, 10.4, 9.4, 9.1, 8.6, 7.2, 8.7, 6.3, 9.3 };

        var sut = Dtw.GetPath(a, b);

        const double expectedScoreResult = 68.9;
        Assert.IsTrue(Math.Abs(sut.Score - expectedScoreResult) < GlobalConstants.DoubleTolerance);

        var expectedPath = new List<Tuple<int, int>>
        {
            Tuple.Create(0, 0),
            Tuple.Create(1, 1),
            Tuple.Create(2, 2),
            Tuple.Create(3, 3),
            Tuple.Create(4, 4),
            Tuple.Create(4, 5),
            Tuple.Create(4, 6),
            Tuple.Create(4, 7),
            Tuple.Create(4, 8),
            Tuple.Create(5, 9),
            Tuple.Create(6, 10),
            Tuple.Create(7, 11)
        };

        for (var i = 0; i < expectedPath.Count; i++)
        {
            Assert.AreEqual(expectedPath[i].Item1, sut.Path[i].Item1);
            Assert.AreEqual(expectedPath[i].Item2, sut.Path[i].Item2);
        }
    }

    [TestMethod]
    public void GetPathForEqualArrays()
    {
        var a = new double[] { 1, 2, 3, 4 };
        var b = new double[] { 1, 2, 3, 4 };

        var sut = Dtw.GetPath(a, b);

        Assert.AreEqual(0, sut.Score);

        var expectedPath = new List<Tuple<int, int>>
        {
            Tuple.Create(0, 0),
            Tuple.Create(1, 1),
            Tuple.Create(2, 2),
            Tuple.Create(3, 3)
        };

        for (var i = 0; i < expectedPath.Count; i++)
        {
            Assert.AreEqual(expectedPath[i].Item1, sut.Path[i].Item1);
            Assert.AreEqual(expectedPath[i].Item2, sut.Path[i].Item2);
        }
    }

    [TestMethod]
    public void GetPathForZeroArrayA()
    {
        var a = new double[] { 0, 0, 0, 0, 0 };
        var b = new double[] { 0, 0, 0, 0 };
        var sut = Dtw.GetPath(a, b);

        Assert.AreEqual(0, sut.Score);

        var expectedPath = new List<Tuple<int, int>>
        {
            Tuple.Create(0, 0),
            Tuple.Create(1, 0),
            Tuple.Create(2, 1),
            Tuple.Create(3, 2),
            Tuple.Create(4, 3)
        };

        for (var i = 0; i < expectedPath.Count; i++)
        {
            Assert.AreEqual(expectedPath[i].Item1, sut.Path[i].Item1);
            Assert.AreEqual(expectedPath[i].Item2, sut.Path[i].Item2);
        }
    }

    [TestMethod]
    public void GetPathForZeroArrayB()
    {
        var a = new double[] { 0, 0, 0, 0 };
        var b = new double[] { 0, 0, 0, 0, 0 };
        var sut = Dtw.GetPath(a, b);

        Assert.AreEqual(0, sut.Score);

        var expectedPath = new List<Tuple<int, int>>
        {
            Tuple.Create(0, 0),
            Tuple.Create(0, 1),
            Tuple.Create(1, 2),
            Tuple.Create(2, 3),
            Tuple.Create(3, 4)
        };

        for (var i = 0; i < expectedPath.Count; i++)
        {
            Assert.AreEqual(expectedPath[i].Item1, sut.Path[i].Item1);
            Assert.AreEqual(expectedPath[i].Item2, sut.Path[i].Item2);
        }
    }

    [TestMethod]
    public void GetPathForEqualZeroArray()
    {
        var a = new double[] { 0, 0, 0, 0 };
        var b = new double[] { 0, 0, 0, 0 };
        var sut = Dtw.GetPath(a, b);

        Assert.AreEqual(0, sut.Score);

        var expectedPath = new List<Tuple<int, int>>
        {
            Tuple.Create(0, 0),
            Tuple.Create(1, 1),
            Tuple.Create(2, 2),
            Tuple.Create(3, 3)
        };

        for (var i = 0; i < expectedPath.Count; i++)
        {
            Assert.AreEqual(expectedPath[i].Item1, sut.Path[i].Item1);
            Assert.AreEqual(expectedPath[i].Item2, sut.Path[i].Item2);
        }
    }
}