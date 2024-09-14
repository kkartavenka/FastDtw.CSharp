using System;

namespace FastDtw.CSharp.Implementations.Shared
{
    internal static class InputArrayValidator
    {
        internal static void ValidateLength<T>(T[] arrayA, T[] arrayB) where T : struct
        {
            if (arrayA.Length < 2)
            {
                throw new ArgumentException("Array length, should be at least 2", nameof(arrayA));
            }

            if (arrayB.Length < 2)
            {
                throw new ArgumentException("Array length, should be at least 2", nameof(arrayB));
            }
        }
        
        internal static void ValidateLength<T>(T[] arrayA, T[] arrayB, T[] weightsA, T[] weightsB)
            where T : struct
        {
            if (arrayA.Length < 2)
            {
                throw new ArgumentException("Span length, should be at least 2", nameof(arrayA));
            }

            if (arrayB.Length < 2)
            {
                throw new ArgumentException("Span length, should be at least 2", nameof(arrayB));
            }

            if (arrayA.Length != weightsA.Length)
            {
                throw new ArgumentException("Weight weightsA length should be equal to arrayA length",
                    nameof(weightsA));
            }

            if (arrayB.Length != weightsB.Length)
            {
                throw new ArgumentException("Weight weightsB length should be equal to arrayA length",
                    nameof(weightsB));
            }
        }

#if NET6_0_OR_GREATER
        internal static void ValidateLength<T>(Span<T> arrayA, Span<T> arrayB) where T : struct
        {
            if (arrayA.Length < 2)
            {
                throw new ArgumentException("Span length, should be at least 2", nameof(arrayA));
            }

            if (arrayB.Length < 2)
            {
                throw new ArgumentException("Span length, should be at least 2", nameof(arrayB));
            }
        }

        internal static void ValidateLength<T>(Span<T> arrayA, Span<T> arrayB, Span<T> weightsA, Span<T> weightsB)
            where T : struct
        {
            if (arrayA.Length < 2)
            {
                throw new ArgumentException("Span length, should be at least 2", nameof(arrayA));
            }

            if (arrayB.Length < 2)
            {
                throw new ArgumentException("Span length, should be at least 2", nameof(arrayB));
            }

            if (arrayA.Length != weightsA.Length)
            {
                throw new ArgumentException("Weight weightsA length should be equal to arrayA length",
                    nameof(weightsA));
            }

            if (arrayB.Length != weightsB.Length)
            {
                throw new ArgumentException("Weight weightsB length should be equal to arrayA length",
                    nameof(weightsB));
            }
        }
#endif
    }
}