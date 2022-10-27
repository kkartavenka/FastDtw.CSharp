using ILGPU;
using ILGPU.Runtime;
using ILGPU.Runtime.Cuda;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastDtw.CSharp; 
public static class DtwGpu {

    public static double GetScore(double[] arrayA, double[] arrayB) {
        var (aLength, bLength) = (arrayA.Length + 1, arrayB.Length + 1);

        using var context = Context.CreateDefault();
        using var accelerator = context.GetCudaDevice(0).CreateCudaAccelerator(context);

        var allocArrayA = accelerator.Allocate1D(arrayA);
        var allocArrayB = accelerator.Allocate1D(arrayB);
        var allocDtwBuffer = accelerator.Allocate2DDenseY<double>(new LongIndex2D(aLength, bLength));

        var loadedKernel = accelerator.LoadAutoGroupedStreamKernel<
            Index2D,
            ArrayView1D<double, Stride1D.Dense>,
            ArrayView1D<double, Stride1D.Dense>,
            ArrayView2D<double, Stride2D.DenseY>>(ExecuteKernel);

        loadedKernel(allocDtwBuffer.Extent.ToIntIndex(), allocArrayA, allocArrayB, allocDtwBuffer);

        return 0;
    }

    private static void ExecuteKernel(Index2D arg1, ArrayView1D<double, Stride1D.Dense> arg2, ArrayView1D<double, Stride1D.Dense> arg3, ArrayView2D<double, Stride2D.DenseY> arg4) {
        var aLength = arg2.Length;
        var bLength = arg3.Length;

        for (var i = 1; i < aLength; i++) {
            for (var j = 1; j < bLength; j++) {
                var cost = Math.Abs(arg2[i - 1] - arg3[j - 1]);

                double lastMin;
                if (i == 1 && j == 1)
                    lastMin = arg4[i - 1, j - 1];
                else if (i == 1)
                    lastMin = arg4[i, j - 1];
                else if (j == 1)
                    lastMin = arg4[i - 1, j];
                else
                    lastMin = Math.Min(arg4[i - 1, j], Math.Min(arg4[i, j - 1], arg4[i - 1, j - 1]));

                arg4[i, j] = cost + lastMin;
            }
        }

        //Interop.WriteLine($"A: {aLength}, B: {bLength}");
        Interop.WriteLine("{0}", arg4[aLength - 1, bLength - 1]);
    }
}
