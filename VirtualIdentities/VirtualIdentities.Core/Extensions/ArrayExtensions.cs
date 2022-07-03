using System.Diagnostics.CodeAnalysis;

namespace VirtualIdentities.Core.Extensions
{
    internal static class ArrayExtensions
    {
        public static T[] ConcatArray<T>(this T[] array1, T[] array2)
        {
            var result = new T[array1.Length + array2.Length];
            array1.CopyTo(result, 0);
            array2.CopyTo(result, array1.Length);
            return result;
        }

        public static T[] ConcatArray<T>(this T[] array1, T[] array2, T[] array3)
        {
            var length12 = array1.Length + array2.Length;
            var result = new T[length12 + array3.Length];
            array1.CopyTo(result, 0);
            array2.CopyTo(result, array1.Length);
            array2.CopyTo(result, length12);
            return result;
        }

        public static bool CheckPrefix<T>(
            this T[] array, T[] prefix, [NotNullWhen(true)] out Span<T> span)
            where T : IEquatable<T>
        {
            if (array.Length < prefix.Length)
            {
                span = null;
                return false;
            }

            for (int i = 0; i < prefix.Length; i++)
            {
                if (!array[i].Equals(prefix[i]))
                {
                    span = null;
                    return false;
                }
            }

            span = array.AsSpan(prefix.Length);
            return true;
        }
    }
}
