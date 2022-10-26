using FastDtw.CSharp;

namespace FastDtwTest; 
[TestClass]
public class UnitTest {
    private const string _testFile = @"C:\Users\kkart\source\repos\FastDtw.CSharp\Data\Test.csv";
    private const double _floatDeviation = 1e-5;

    [TestMethod]
    public void CrossValidateDouble() {
        var data = GetData();
        var fastDtw = FastDtw.Dtw.Distance(data.arrayA, data.arrayB);
        var fastDtwCSharp = Dtw.GetScore(data.arrayA, data.arrayB);
        Assert.IsTrue(fastDtw == fastDtwCSharp);
    }

    [TestMethod]
    public void CrossValidateFloat() {
        var data = GetData();
        var fastDtw = FastDtw.Dtw.Distance(data.arrayA, data.arrayB);
        var fastDtwCSharpF = Dtw.GetScoreF(data.arrayAF, data.arrayBF);

        var ratio = fastDtw / fastDtwCSharpF;
        if (ratio > 1)
            ratio -= 1;
        else
            ratio = 1 - ratio;

        Assert.IsTrue(ratio < _floatDeviation);
    }

    private (double[] arrayA, double[] arrayB, float[] arrayAF, float[] arrayBF) GetData() {
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

}