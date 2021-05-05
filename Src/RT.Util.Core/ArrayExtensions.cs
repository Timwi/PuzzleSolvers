using System;
using System.Collections.Generic;

namespace RT.Util.ExtensionMethods
{
    /// <summary>Provides extension methods on array types.</summary>
#if EXPORT_UTIL
    public
#endif
    static class ArrayExtensions
    {
        /// <summary>
        ///     Similar to <see cref="string.Substring(int)"/>, but for arrays. Returns a new array containing all items from
        ///     the specified <paramref name="startIndex"/> onwards.</summary>
        /// <remarks>
        ///     Returns a new copy of the array even if <paramref name="startIndex"/> is 0.</remarks>
        public static T[] Subarray<T>(this T[] array, int startIndex)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            return Subarray(array, startIndex, array.Length - startIndex);
        }

        /// <summary>
        ///     Similar to <see cref="string.Substring(int,int)"/>, but for arrays. Returns a new array containing <paramref
        ///     name="length"/> items from the specified <paramref name="startIndex"/> onwards.</summary>
        /// <remarks>
        ///     Returns a new copy of the array even if <paramref name="startIndex"/> is 0 and <paramref name="length"/> is
        ///     the length of the input array.</remarks>
        public static T[] Subarray<T>(this T[] array, int startIndex, int length)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            if (startIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(startIndex), "startIndex cannot be negative.");
            if (length < 0 || startIndex + length > array.Length)
                throw new ArgumentOutOfRangeException(nameof(length), "length cannot be negative or extend beyond the end of the array.");
            T[] result = new T[length];
            Array.Copy(array, startIndex, result, 0, length);
            return result;
        }

        /// <summary>
        ///     Similar to <see cref="string.Remove(int)"/>, but for arrays. Returns a new array containing only the items
        ///     before the specified <paramref name="startIndex"/>.</summary>
        /// <remarks>
        ///     Returns a new copy of the array even if <paramref name="startIndex"/> is the length of the array.</remarks>
        public static T[] Remove<T>(this T[] array, int startIndex)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            if (startIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(startIndex), "startIndex cannot be negative.");
            if (startIndex > array.Length)
                throw new ArgumentOutOfRangeException(nameof(startIndex), "startIndex cannot be greater than the length of the array.");
            T[] result = new T[startIndex];
            Array.Copy(array, 0, result, 0, startIndex);
            return result;
        }

        /// <summary>
        ///     Similar to <see cref="string.Remove(int,int)"/>, but for arrays. Returns a new array containing everything
        ///     except the <paramref name="length"/> items starting from the specified <paramref name="startIndex"/>.</summary>
        /// <remarks>
        ///     Returns a new copy of the array even if <paramref name="length"/> is 0.</remarks>
        public static T[] Remove<T>(this T[] array, int startIndex, int length)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            if (startIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(startIndex), "startIndex cannot be negative.");
            if (length < 0 || startIndex + length > array.Length)
                throw new ArgumentOutOfRangeException(nameof(length), "length cannot be negative or extend beyond the end of the array.");
            T[] result = new T[array.Length - length];
            Array.Copy(array, 0, result, 0, startIndex);
            Array.Copy(array, startIndex + length, result, startIndex, array.Length - length - startIndex);
            return result;
        }

        /// <summary>
        ///     Similar to <see cref="string.Insert(int, string)"/>, but for arrays. Returns a new array with the <paramref
        ///     name="values"/> inserted starting from the specified <paramref name="startIndex"/>.</summary>
        /// <remarks>
        ///     Returns a new copy of the array even if <paramref name="values"/> is empty.</remarks>
        public static T[] Insert<T>(this T[] array, int startIndex, params T[] values)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            if (values == null)
                throw new ArgumentNullException(nameof(values));
            if (startIndex < 0 || startIndex > array.Length)
                throw new ArgumentOutOfRangeException(nameof(startIndex), "startIndex must be between 0 and the size of the input array.");
            T[] result = new T[array.Length + values.Length];
            Array.Copy(array, 0, result, 0, startIndex);
            Array.Copy(values, 0, result, startIndex, values.Length);
            Array.Copy(array, startIndex, result, startIndex + values.Length, array.Length - startIndex);
            return result;
        }

        /// <summary>
        ///     Similar to <see cref="string.Insert(int, string)"/>, but for arrays and for a single value. Returns a new
        ///     array with the <paramref name="value"/> inserted at the specified <paramref name="startIndex"/>.</summary>
        public static T[] Insert<T>(this T[] array, int startIndex, T value)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            if (startIndex < 0 || startIndex > array.Length)
                throw new ArgumentOutOfRangeException(nameof(startIndex), "startIndex must be between 0 and the size of the input array.");
            T[] result = new T[array.Length + 1];
            Array.Copy(array, 0, result, 0, startIndex);
            result[startIndex] = value;
            Array.Copy(array, startIndex, result, startIndex + 1, array.Length - startIndex);
            return result;
        }

        /// <summary>
        ///     Concatenates two arrays.</summary>
        /// <remarks>
        ///     Returns a new copy of the array even if one of the input arrays is empty.</remarks>
        public static T[] Concat<T>(this T[] array, T[] otherArray)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            if (otherArray == null)
                throw new ArgumentNullException(nameof(otherArray));
            if (otherArray.Length == 0)
                return (T[]) array.Clone();
            if (array.Length == 0)
                return (T[]) otherArray.Clone();
            T[] result = new T[array.Length + otherArray.Length];
            Array.Copy(array, 0, result, 0, array.Length);
            Array.Copy(otherArray, 0, result, array.Length, otherArray.Length);
            return result;
        }

        /// <summary>
        ///     Returns a new array in which a single element has been replaced.</summary>
        /// <param name="array">
        ///     The array from which to create a new array with one element replaced.</param>
        /// <param name="index">
        ///     The index at which to replace one element.</param>
        /// <param name="element">
        ///     The new element to replace the old element with.</param>
        public static T[] Replace<T>(this T[] array, int index, T element)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            if (index < 0 || index > array.Length)
                throw new ArgumentOutOfRangeException(nameof(index), "index must be between 0 and the size of the input array.");
            var result = (T[]) array.Clone();
            result[index] = element;
            return result;
        }

        /// <summary>
        ///     Determines whether a subarray within the current array is equal to the specified other array.</summary>
        /// <param name="sourceArray">
        ///     First array to examine.</param>
        /// <param name="sourceStartIndex">
        ///     Start index of the subarray within the first array to compare.</param>
        /// <param name="otherArray">
        ///     Array to compare the subarray against.</param>
        /// <param name="comparer">
        ///     Optional equality comparer.</param>
        /// <returns>
        ///     True if the current array contains the specified subarray at the specified index; false otherwise.</returns>
        public static bool SubarrayEquals<T>(this T[] sourceArray, int sourceStartIndex, T[] otherArray, IEqualityComparer<T> comparer = null)
        {
            if (otherArray == null)
                throw new ArgumentNullException(nameof(otherArray));
            return SubarrayEquals(sourceArray, sourceStartIndex, otherArray, 0, otherArray.Length, comparer);
        }

        /// <summary>
        ///     Determines whether the two arrays contain the same content in the specified location.</summary>
        /// <param name="sourceArray">
        ///     First array to examine.</param>
        /// <param name="sourceStartIndex">
        ///     Start index of the subarray within the first array to compare.</param>
        /// <param name="otherArray">
        ///     Second array to examine.</param>
        /// <param name="otherStartIndex">
        ///     Start index of the subarray within the second array to compare.</param>
        /// <param name="length">
        ///     Length of the subarrays to compare.</param>
        /// <param name="comparer">
        ///     Optional equality comparer.</param>
        /// <returns>
        ///     True if the two arrays contain the same subarrays at the specified indexes; false otherwise.</returns>
        public static bool SubarrayEquals<T>(this T[] sourceArray, int sourceStartIndex, T[] otherArray, int otherStartIndex, int length, IEqualityComparer<T> comparer = null)
        {
            if (sourceArray == null)
                throw new ArgumentNullException(nameof(sourceArray));
            if (sourceStartIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(sourceStartIndex), "The sourceStartIndex argument must be non-negative.");
            if (otherArray == null)
                throw new ArgumentNullException(nameof(otherArray));
            if (otherStartIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(otherStartIndex), "The otherStartIndex argument must be non-negative.");
            if (length < 0 || sourceStartIndex + length > sourceArray.Length || otherStartIndex + length > otherArray.Length)
                throw new ArgumentOutOfRangeException(nameof(length), "The length argument must be non-negative and must be such that both subarrays are within the bounds of the respective source arrays.");

            if (comparer == null)
                comparer = EqualityComparer<T>.Default;
            for (int i = 0; i < length; i++)
                if (!comparer.Equals(sourceArray[sourceStartIndex + i], otherArray[otherStartIndex + i]))
                    return false;
            return true;
        }

        /// <summary>
        ///     Searches the current array for a specified subarray and returns the index of the first occurrence, or -1 if
        ///     not found.</summary>
        /// <param name="sourceArray">
        ///     Array in which to search for the subarray.</param>
        /// <param name="findWhat">
        ///     Subarray to search for.</param>
        /// <param name="comparer">
        ///     Optional equality comparer.</param>
        /// <returns>
        ///     The index of the first match, or -1 if no match is found.</returns>
        public static int IndexOfSubarray<T>(this T[] sourceArray, T[] findWhat, IEqualityComparer<T> comparer = null)
        {
            if (sourceArray == null)
                throw new ArgumentNullException(nameof(sourceArray));
            if (findWhat == null)
                throw new ArgumentNullException(nameof(findWhat));

            for (int i = 0; i <= sourceArray.Length - findWhat.Length; i++)
                if (sourceArray.SubarrayEquals(i, findWhat, 0, findWhat.Length, comparer))
                    return i;
            return -1;
        }

        /// <summary>
        ///     Searches the current array for a specified subarray and returns the index of the first occurrence, or -1 if
        ///     not found.</summary>
        /// <param name="sourceArray">
        ///     Array in which to search for the subarray.</param>
        /// <param name="findWhat">
        ///     Subarray to search for.</param>
        /// <param name="startIndex">
        ///     Index in <paramref name="sourceArray"/> at which to start searching.</param>
        /// <param name="sourceLength">
        ///     Maximum length of the source array to search starting from <paramref name="startIndex"/>. The greatest index
        ///     that can be returned is this minus the length of <paramref name="findWhat"/> plus <paramref
        ///     name="startIndex"/>.</param>
        /// <param name="comparer">
        ///     Optional equality comparer.</param>
        /// <returns>
        ///     The index of the first match, or -1 if no match is found.</returns>
        public static int IndexOfSubarray<T>(this T[] sourceArray, T[] findWhat, int startIndex, int? sourceLength = null, IEqualityComparer<T> comparer = null)
        {
            if (sourceArray == null)
                throw new ArgumentNullException(nameof(sourceArray));
            if (findWhat == null)
                throw new ArgumentNullException(nameof(findWhat));
            if (startIndex < 0 || startIndex > sourceArray.Length)
                throw new ArgumentOutOfRangeException("startIndex");
            if (sourceLength != null && (sourceLength < 0 || sourceLength + startIndex > sourceArray.Length))
                throw new ArgumentOutOfRangeException("sourceLength");

            var maxIndex = (sourceLength == null ? sourceArray.Length : startIndex + sourceLength.Value) - findWhat.Length;
            for (int i = startIndex; i <= maxIndex; i++)
                if (sourceArray.SubarrayEquals(i, findWhat, 0, findWhat.Length, comparer))
                    return i;
            return -1;
        }

        /// <summary>Reverses an array in-place and returns the same array.</summary>
        public static T[] ReverseInplace<T>(this T[] input)
        {
            Array.Reverse(input);
            return input;
        }
    }
}
