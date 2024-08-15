using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FastDtw.CSharp.Test;

[TestClass]
public class EdgeCaseTests
{
    [TestMethod]
    public void LengthRequirement()
    {
        Assert.ThrowsException<ArgumentException>(() => Dtw.GetScore(new double[1], new double[1]));
        Assert.ThrowsException<ArgumentException>(() => Dtw.GetScore(new double[2], new double[1]));
        
        Assert.ThrowsException<ArgumentException>(() => Dtw.GetScoreF(new float[1], new float[1]));
        Assert.ThrowsException<ArgumentException>(() => Dtw.GetScoreF(new float[2], new float[1]));
    }
}