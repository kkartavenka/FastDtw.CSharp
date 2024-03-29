namespace FastDtw.CSharp.Test
{
    [TestClass]
    public class Tests
    {
        private const string _testFile = @"C:\Users\kkart\source\repos\FastDtw.CSharp\Data\Test.csv";
        private double[] _arrayA, _arrayB;
        private float[] _arrayAF, _arrayBF;

        private static (double[] arrayA, double[] arrayB, float[] arrayAF, float[] arrayBF) GetData()
        {
            var lines = File.ReadAllLines(_testFile);

            var arrayA = new double[lines.Length];
            var arrayB = new double[lines.Length];
            var arrayAF = new float[lines.Length];
            var arrayBF = new float[lines.Length];

            int idxA = 0, idxB = 0;
            foreach (var line in lines)
            {
                var splittedString = line.Split(',');

                if (double.TryParse(splittedString[0], out double varA))
                {
                    arrayAF[idxA] = (float)varA;
                    arrayA[idxA++] = varA;
                }

                if (double.TryParse(splittedString[1], out double varB))
                {
                    arrayBF[idxB] = (float)varB;
                    arrayB[idxB++] = varB;
                }
            }

            if (idxA != arrayA.Length)
            {
                arrayA = arrayA[0..idxA];
                arrayAF = arrayAF[0..idxA];
            }

            if (idxB != arrayB.Length)
            {
                arrayB = arrayB[0..idxB];
                arrayBF = arrayBF[0..idxB];
            }

            return (arrayA[0..idxA], arrayB[0..idxB], arrayAF[0..idxA], arrayBF[0..idxB]);
        }

        [TestInitialize]
        public void Init() => (_arrayA, _arrayB, _arrayAF, _arrayBF) = GetData();

        [TestMethod]
        public void EqualLength()
        {
            var a = new double[] { 41.98, 41.65, 42.01, 42.35, 44.4, 43.08, 43.6, 42.84, 42.83, 44.01, 43.07, 44.3, 44.6, 46.54, 45.06, 44.96, 43.59, 46.84, 45.22, 45.52 };
            var b = new double[] { 95.07, 93.5, 96.67, 96.28, 102.47, 94.24, 95.12, 87.06, 87.92, 88.73, 86.36, 95.34, 93.87, 99.42, 91.13, 89.84, 85.52, 89.64, 84.91, 85.16 };

            Assert.AreEqual(Dtw.GetScore(a, b), 959.79999999999984);
        }

        [TestMethod]
        public void UnequalLength()
        {
            var a = new double[] { 41.98, 41.65, 42.01, 42.35, 44.4, 43.08, 43.6, 42.84, 42.83, 44.01, 43.07, 44.3, 44.6, 46.54, 45.06, 44.96, 43.59, 46.84, 45.22, 45.52 };
            var b = new double[] { 95.07, 93.5, 96.67, 96.28, 102.47, 94.24, 95.12, 87.06, 87.92, 88.73, 86.36, 95.34, 93.87, 99.42 };

            Assert.AreEqual(Dtw.GetScore(a, b), 951.76);
        }

        [TestMethod]
        public void CrossValidateShort()
        {
            Assert.AreEqual(FastDtw.Distance(_arrayA[0..50], _arrayB[0..50], 50), Dtw.GetScore(_arrayA[0..50], _arrayB[0..50]));
        }

        [TestMethod]
        public void CrossValidateMedium()
        {
            Assert.AreEqual(FastDtw.Distance(_arrayA[0..500], _arrayB[0..500], 500), Dtw.GetScore(_arrayA[0..500], _arrayB[0..500]));
        }

        [TestMethod]
        public void CrossValidateFull()
        {
            var r1 = FastDtw.Distance(_arrayA, _arrayB, Math.Max(_arrayA.Length, _arrayB.Length));
            var r2 = Dtw.GetScore(_arrayA, _arrayB);
            Assert.AreEqual(r1, r2);
        }

        [TestMethod]
        public void EqualLengthF()
        {
            var a = new float[] { 41.98f, 41.65f, 42.01f, 42.35f, 44.4f, 43.08f, 43.6f, 42.84f, 42.83f, 44.01f, 43.07f, 44.3f, 44.6f, 46.54f, 45.06f, 44.96f, 43.59f, 46.84f, 45.22f, 45.52f };
            var b = new float[] { 95.07f, 93.5f, 96.67f, 96.28f, 102.47f, 94.24f, 95.12f, 87.06f, 87.92f, 88.73f, 86.36f, 95.34f, 93.87f, 99.42f, 91.13f, 89.84f, 85.52f, 89.64f, 84.91f, 85.16f };

            Assert.AreEqual(Dtw.GetScoreF(a, b), 959.800048828125);
        }

        [TestMethod]
        public void UnequalLengthF()
        {
            var a = new float[] { 41.98f, 41.65f, 42.01f, 42.35f, 44.4f, 43.08f, 43.6f, 42.84f, 42.83f, 44.01f, 43.07f, 44.3f, 44.6f, 46.54f, 45.06f, 44.96f, 43.59f, 46.84f, 45.22f, 45.52f };
            var b = new float[] { 95.07f, 93.5f, 96.67f, 96.28f, 102.47f, 94.24f, 95.12f, 87.06f, 87.92f, 88.73f, 86.36f, 95.34f, 93.87f, 99.42f };

            Assert.AreEqual(Dtw.GetScoreF(a, b), 951.7601318359375);
        }

        [TestMethod]
        public void CrossValidateShortF()
        {
            var r1 = FastDtw.Distance(_arrayA[0..50], _arrayB[0..50], 50);
            var r2 = Dtw.GetScoreF(_arrayAF[0..50], _arrayBF[0..50]);
            Assert.IsTrue(Math.Max(r1, r2) / Math.Min(r1, r2) < 1.00001);
        }

        [TestMethod]
        public void CrossValidateMediumF()
        {
            var r1 = FastDtw.Distance(_arrayA[0..500], _arrayB[0..500], 500);
            var r2 = Dtw.GetScoreF(_arrayAF[0..500], _arrayBF[0..500]);
            Assert.IsTrue(Math.Max(r1, r2) / Math.Min(r1, r2) < 1.00001);
        }

        [TestMethod]
        public void CrossValidateFullF()
        {
            var r1 = FastDtw.Distance(_arrayA, _arrayB, Math.Max(_arrayA.Length, _arrayB.Length));
            var r2 = Dtw.GetScoreF(_arrayAF, _arrayBF);
            Assert.IsTrue(Math.Max(r1, r2) / Math.Min(r1, r2) < 1.00001);
        }

    }
}