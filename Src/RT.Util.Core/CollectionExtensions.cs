using System;
using System.Collections.Generic;
using System.Linq;

namespace RT.Util.ExtensionMethods
{
    /// <summary>
    ///     Provides extension methods on various collection types or interfaces in the System.Collections.Generic namespace
    ///     such as <see cref="IList{T}"/>.</summary>
#if EXPORT_UTIL
    public
#endif
    static class CollectionExtensions
    {
        /// <summary>
        ///     Performs a binary search for the specified key on a <see cref="SortedList&lt;TK,TV&gt;"/>. When no match
        ///     exists, returns the nearest indices for interpolation/extrapolation purposes.</summary>
        /// <remarks>
        ///     If an exact match exists, index1 == index2 == the index of the match. If an exact match is not found, index1
        ///     &lt; index2. If the key is less than every key in the list, index1 is int.MinValue and index2 is 0. If it's
        ///     greater than every key, index1 = last item index and index2 = int.MaxValue. Otherwise index1 and index2 are
        ///     the indices of the items that would surround the key were it present in the list.</remarks>
        /// <param name="list">
        ///     List to operate on.</param>
        /// <param name="key">
        ///     The key to look for.</param>
        /// <param name="index1">
        ///     Receives the value of the first index (see remarks).</param>
        /// <param name="index2">
        ///     Receives the value of the second index (see remarks).</param>
        public static void BinarySearch<TK, TV>(this SortedList<TK, TV> list, TK key, out int index1, out int index2)
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list));
            if (key == null)
                throw new ArgumentNullException(nameof(key), "Null values cannot be used for keys in SortedList.");

            var keys = list.Keys;
            var comparer = Comparer<TK>.Default;

            int imin = 0;
            int imax = (0 + keys.Count) - 1;
            while (imin <= imax)
            {
                int inew = imin + ((imax - imin) >> 1);

                int cmp_res;
                try { cmp_res = comparer.Compare(keys[inew], key); }
                catch (Exception exception) { throw new InvalidOperationException("SortedList.BinarySearch could not compare keys due to a comparer exception.", exception); }

                if (cmp_res == 0)
                {
                    index1 = index2 = inew;
                    return;
                }
                else if (cmp_res < 0)
                {
                    imin = inew + 1;
                }
                else
                {
                    imax = inew - 1;
                }
            }

            index1 = imax; // we know that imax + 1 == imin
            index2 = imin;
            if (imax < 0)
                index1 = int.MinValue;
            if (imin >= keys.Count)
                index2 = int.MaxValue;
        }

        /// <summary>
        ///     Enqueues several values into a <see cref="Queue&lt;T&gt;"/>.</summary>
        /// <typeparam name="T">
        ///     Type of the elements in the queue.</typeparam>
        /// <param name="queue">
        ///     Queue to insert items into.</param>
        /// <param name="values">
        ///     Values to enqueue.</param>
        public static void EnqueueRange<T>(this Queue<T> queue, IEnumerable<T> values)
        {
            foreach (var value in values)
                queue.Enqueue(value);
        }

        /// <summary>
        ///     Adds several values into a <see cref="HashSet&lt;T&gt;"/>.</summary>
        /// <typeparam name="T">
        ///     Type of the elements in the hash set.</typeparam>
        /// <param name="set">
        ///     The set to add the items to.</param>
        /// <param name="values">
        ///     Values to add.</param>
        public static void AddRange<T>(this HashSet<T> set, IEnumerable<T> values)
        {
            foreach (var value in values)
                set.Add(value);
        }

        /// <summary>
        ///     Removes several values from a <see cref="List&lt;T&gt;"/>.</summary>
        /// <typeparam name="T">
        ///     Type of the elements in the list.</typeparam>
        /// <param name="list">
        ///     The list to remove the items from.</param>
        /// <param name="values">
        ///     Values to remove.</param>
        public static void RemoveRange<T>(this List<T> list, IEnumerable<T> values)
        {
            foreach (var value in values)
                list.Remove(value);
        }

        /// <summary>
        ///     Pops the specified number of elements from the stack. There must be at least that many items on the stack,
        ///     otherwise an exception is thrown.</summary>
        public static void Pop<T>(this Stack<T> stack, int count)
        {
            for (int i = 0; i < count; i++)
                stack.Pop();
        }
    }
}
