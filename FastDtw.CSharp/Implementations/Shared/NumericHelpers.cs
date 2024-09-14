using System.Runtime.CompilerServices;

namespace FastDtw.CSharp.Implementations.Shared
{
    internal static class NumericHelpers
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static double FindMinimum(ref double prevRowFirst, ref double prevRowSecond,
            ref double currentRowSecond)
        {
            if (prevRowFirst < prevRowSecond)
            {
                return prevRowFirst < currentRowSecond ? prevRowFirst : currentRowSecond;
            }

            return prevRowSecond < currentRowSecond ? prevRowSecond : currentRowSecond;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static float FindMinimumF(ref float prevRowFirst, ref float prevRowSecond, ref float currentRowSecond)
        {
            if (prevRowFirst < prevRowSecond)
            {
                return prevRowFirst < currentRowSecond ? prevRowFirst : currentRowSecond;
            }

            return prevRowSecond < currentRowSecond ? prevRowSecond : currentRowSecond;
        }
    }
}