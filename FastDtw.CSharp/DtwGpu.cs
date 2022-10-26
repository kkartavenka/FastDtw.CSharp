using ILGPU;
using ILGPU.Runtime;
using ILGPU.Runtime.Cuda;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastDtw.CSharp; 
internal class DtwGpu {
    static void Kernel(Index1D i, Index1D j, ArrayView<double> arrayA, ArrayView<double> arrayB) {

    }

    public double GetScore(double[] arrayA, double[] arrayB) {
        using var context = Context.Create(_ => _.EnableAlgorithms());
        using var accelerator = context.GetPreferredDevice(preferCPU: false).CreateAccelerator(context);

        var allocArrayA = accelerator.Allocate1D(arrayA);
        var allocArrayB = accelerator.Allocate1D(arrayB);
        var allocDtw = accelerator.Allocate2D()

        Action<Index1D, Index1D, ArrayView<double>, ArrayView<double>> loadedKernel =
           accelerator.LoadAutoGroupedStreamKernel<Index1D, Index1D, ArrayView<double>, ArrayView<double>>(Kernel);



        var (aLength, bLength) = (arrayA.Length + 1, arrayB.Length + 1);

        var spanA = arrayA.AsSpan();
        var spanB = arrayB.AsSpan();

        double[][] dtw = new double[aLength][];
        dtw[0] = new double[bLength];

        double[] currentDtwRow;
        var previousDtwRow = dtw[0];

        for (var i = 1; i < aLength; i++) {
            currentDtwRow = new double[bLength];

            for (var j = 1; j < bLength; j++) {
                var cost = Math.Abs(spanA[i - 1] - spanB[j - 1]);

                double lastMin;
                if (i == 1 && j == 1)
                    lastMin = previousDtwRow[j - 1];
                else if (i == 1)
                    lastMin = currentDtwRow[j - 1];
                else if (j == 1)
                    lastMin = previousDtwRow[j];
                else
                    lastMin = Math.Min(previousDtwRow[j], Math.Min(currentDtwRow[j - 1], previousDtwRow[j - 1]));

                currentDtwRow[j] = cost + lastMin;
            }
            dtw[i] = currentDtwRow;
            previousDtwRow = currentDtwRow;
        }

        return dtw[aLength - 1][bLength - 1];
    }

}
