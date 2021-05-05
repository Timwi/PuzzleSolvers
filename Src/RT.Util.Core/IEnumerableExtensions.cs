using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RT.Util.ExtensionMethods
{
    /// <summary>Provides extension methods on the <see cref="IEnumerable&lt;T&gt;"/> type.</summary>
#if EXPORT_UTIL
    public
#endif
    static class IEnumerableExtensions
    {
        /// <summary>
        ///     Returns an enumeration of tuples containing all pairs of elements from the source collection. For example, the
        ///     input sequence 1, 2 yields the pairs [1,1], [1,2], [2,1], and [2,2].</summary>
        public static IEnumerable<(T, T)> AllPairs<T>(this IEnumerable<T> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            // Make sure that ‘source’ is evaluated only once
            var sourceArr = source as IList<T> ?? source.ToArray();
            return sourceArr.SelectMany(item1 => sourceArr.Select(item2 => (item1, item2)));
        }

        /// <summary>Returns an enumeration of objects computed from all pairs of elements from the source collection.</summary>
        public static IEnumerable<TResult> AllPairs<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TSource, TResult> selector)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            // Make sure that ‘source’ is evaluated only once
            var sourceArr = source as IList<TSource> ?? source.ToArray();
            return sourceArr.SelectMany(item1 => sourceArr.Select(item2 => selector(item1, item2)));
        }

        /// <summary>
        ///     Returns an enumeration of tuples containing all unique pairs of distinct elements from the source collection.
        ///     For example, the input sequence 1, 2, 3 yields the pairs [1,2], [1,3] and [2,3] only.</summary>
        public static IEnumerable<(T, T)> UniquePairs<T>(this IEnumerable<T> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            IEnumerable<(T, T)> uniquePairsIterator()
            {
                // Make sure that ‘source’ is evaluated only once
                var arr = source as IList<T> ?? source.ToArray();
                for (int i = 0; i < arr.Count - 1; i++)
                    for (int j = i + 1; j < arr.Count; j++)
                        yield return (arr[i], arr[j]);
            }
            return uniquePairsIterator();
        }

        /// <summary>
        ///     Returns an enumeration of tuples containing all consecutive pairs of the elements.</summary>
        /// <param name="source">
        ///     The input enumerable.</param>
        /// <param name="closed">
        ///     If true, an additional pair containing the last and first element is included. For example, if the source
        ///     collection contains { 1, 2, 3, 4 } then the enumeration contains { (1, 2), (2, 3), (3, 4) } if <paramref
        ///     name="closed"/> is false, and { (1, 2), (2, 3), (3, 4), (4, 1) } if <paramref name="closed"/> is true.</param>
        public static IEnumerable<(T, T)> ConsecutivePairs<T>(this IEnumerable<T> source, bool closed) => SelectConsecutivePairs(source, closed, (i1, i2) => (i1, i2));

        /// <summary>
        ///     Enumerates all consecutive pairs of the elements.</summary>
        /// <param name="source">
        ///     The input enumerable.</param>
        /// <param name="closed">
        ///     If true, an additional pair containing the last and first element is included. For example, if the source
        ///     collection contains { 1, 2, 3, 4 } then the enumeration contains { (1, 2), (2, 3), (3, 4) } if <paramref
        ///     name="closed"/> is <c>false</c>, and { (1, 2), (2, 3), (3, 4), (4, 1) } if <paramref name="closed"/> is
        ///     <c>true</c>.</param>
        /// <param name="selector">
        ///     The selector function to run each consecutive pair through.</param>
        public static IEnumerable<TResult> SelectConsecutivePairs<T, TResult>(this IEnumerable<T> source, bool closed, Func<T, T, TResult> selector)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (selector == null)
                throw new ArgumentNullException(nameof(selector));

            IEnumerable<TResult> selectConsecutivePairsIterator()
            {
                using (var enumer = source.GetEnumerator())
                {
                    bool any = enumer.MoveNext();
                    if (!any)
                        yield break;
                    T first = enumer.Current;
                    T last = enumer.Current;
                    while (enumer.MoveNext())
                    {
                        yield return selector(last, enumer.Current);
                        last = enumer.Current;
                    }
                    if (closed)
                        yield return selector(last, first);
                }
            }
            return selectConsecutivePairsIterator();
        }

        /// <summary>Sorts the elements of a sequence in ascending order.</summary>
        public static IEnumerable<T> Order<T>(this IEnumerable<T> source)
        {
            return source.OrderBy(k => k);
        }

        /// <summary>Sorts the elements of a sequence in ascending order by using a specified comparer.</summary>
        public static IEnumerable<T> Order<T>(this IEnumerable<T> source, IComparer<T> comparer)
        {
            return source.OrderBy(k => k, comparer);
        }

        /// <summary>
        ///     Splits the specified IEnumerable at every element that satisfies a specified predicate and returns a
        ///     collection containing each sequence of elements in between each pair of such elements. The elements satisfying
        ///     the predicate are not included.</summary>
        /// <param name="splitWhat">
        ///     The collection to be split.</param>
        /// <param name="splitWhere">
        ///     A predicate that determines which elements constitute the separators.</param>
        /// <returns>
        ///     A collection containing the individual pieces taken from the original collection.</returns>
        public static IEnumerable<IEnumerable<T>> Split<T>(this IEnumerable<T> splitWhat, Func<T, bool> splitWhere)
        {
            if (splitWhat == null)
                throw new ArgumentNullException(nameof(splitWhat));
            if (splitWhere == null)
                throw new ArgumentNullException(nameof(splitWhere));

            IEnumerable<IEnumerable<T>> splitIterator()
            {
                var items = new List<T>();
                foreach (var item in splitWhat)
                {
                    if (splitWhere(item))
                    {
                        yield return items;
                        items = new List<T>();
                    }
                    else
                        items.Add(item);
                }
                yield return items;
            }
            return splitIterator();
        }

        /// <summary>
        ///     Adds a single element to the end of an <see cref="IEnumerable{T}"/>.</summary>
        /// <typeparam name="T">
        ///     Type of enumerable to return.</typeparam>
        /// <returns>
        ///     IEnumerable containing all the input elements, followed by the specified additional element.</returns>
        public static IEnumerable<T> Concat<T>(this IEnumerable<T> source, T element)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            IEnumerable<T> concatIterator()
            {
                foreach (var e in source)
                    yield return e;
                yield return element;
            }
            return concatIterator();
        }

        /// <summary>
        ///     Adds a single element to the start of an <see cref="IEnumerable{T}"/>.</summary>
        /// <typeparam name="T">
        ///     Type of enumerable to return.</typeparam>
        /// <returns>
        ///     IEnumerable containing the specified additional element, followed by all the input elements.</returns>
        public static IEnumerable<T> Concat<T>(this T head, IEnumerable<T> tail)
        {
            if (tail == null)
                throw new ArgumentNullException(nameof(tail));
            IEnumerable<T> concatIterator()
            {
                yield return head;
                foreach (var e in tail)
                    yield return e;
            }
            return concatIterator();
        }

        /// <summary>
        ///     This does the same as <see cref="Order{T}(IEnumerable{T})"/>, but it is much faster if you intend to extract
        ///     only the first few items using <see cref="Enumerable.Take"/>.</summary>
        /// <param name="source">
        ///     The sequence to be sorted.</param>
        /// <returns>
        ///     The given <see cref="IEnumerable{T}"/> with its elements sorted progressively.</returns>
        public static IEnumerable<T> OrderLazy<T>(this IEnumerable<T> source)
        {
            return OrderLazy(source, Comparer<T>.Default);
        }

        /// <summary>
        ///     This does the same as <see cref="Order{T}(IEnumerable{T},IComparer{T})"/>, but it is much faster if you intend
        ///     to extract only the first few items using <see cref="Enumerable.Take"/>.</summary>
        /// <param name="source">
        ///     The sequence to be sorted.</param>
        /// <param name="comparer">
        ///     An instance of <see cref="IComparer{T}"/> specifying the comparison to use on the items.</param>
        /// <returns>
        ///     The given IEnumerable&lt;T&gt; with its elements sorted progressively.</returns>
        public static IEnumerable<T> OrderLazy<T>(this IEnumerable<T> source, IComparer<T> comparer)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (comparer == null)
                throw new ArgumentNullException(nameof(comparer));
            var arr = source.ToArray();
            if (arr.Length < 2)
                return arr;
            int[] map = new int[arr.Length];
            for (int i = 0; i < arr.Length; i++)
                map[i] = i;

            IEnumerable<T> quickSort(T[] items, int left, int right)
            {
                int compareForStableSort(T elem1, int elem1Index, T elem2, int elem2Index)
                {
                    int r = comparer.Compare(elem1, elem2);
                    return r != 0 ? r : elem1Index.CompareTo(elem2Index);
                }

                while (left < right)
                {
                    int curleft = left;
                    int curright = right;
                    int pivotIndex = map[curleft + ((curright - curleft) >> 1)];
                    T pivot = items[pivotIndex];
                    do
                    {
                        while ((curleft < map.Length) && compareForStableSort(pivot, pivotIndex, items[map[curleft]], map[curleft]) > 0)
                            curleft++;
                        while ((curright >= 0) && compareForStableSort(pivot, pivotIndex, items[map[curright]], map[curright]) < 0)
                            curright--;
                        if (curleft > curright)
                            break;

                        if (curleft < curright)
                        {
                            int tmp = map[curleft];
                            map[curleft] = map[curright];
                            map[curright] = tmp;
                        }
                        curleft++;
                        curright--;
                    }
                    while (curleft <= curright);
                    if (left < curright)
                        foreach (var s in quickSort(items, left, curright))
                            yield return s;
                    else if (left == curright)
                        yield return items[map[curright]];
                    if (curright + 1 < curleft)
                        yield return items[map[curright + 1]];
                    left = curleft;
                }
                yield return items[map[left]];
            }
            return quickSort(arr, 0, arr.Length - 1);
        }

        /// <summary>
        ///     Returns all permutations of the input <see cref="IEnumerable&lt;T&gt;"/>.</summary>
        /// <param name="source">
        ///     The list of items to permute.</param>
        /// <returns>
        ///     A collection containing all permutations of the input <see cref="IEnumerable&lt;T&gt;"/>.</returns>
        public static IEnumerable<IEnumerable<T>> Permutations<T>(this IEnumerable<T> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            // Ensure that the source IEnumerable is evaluated only once
            return permutations(source as T[] ?? source.ToArray());
        }

        private static IEnumerable<IEnumerable<T>> permutations<T>(IEnumerable<T> source)
        {
            var c = source.Count();
            if (c < 2)
                yield return source;
            else
                for (int i = 0; i < c; i++)
                    foreach (var p in permutations(source.Take(i).Concat(source.Skip(i + 1))))
                        yield return source.Skip(i).Take(1).Concat(p);
        }

        /// <summary>
        ///     Returns all subsequences of the specified lengths of the input <see cref="IEnumerable&lt;T&gt;"/>.</summary>
        /// <param name="source">
        ///     The sequence of items to generate subsequences of.</param>
        /// <param name="minLength">
        ///     The minimum length of a subsequence to return. Must be between 0 and the length of the input collection.</param>
        /// <param name="maxLength">
        ///     The maximum length of a subsequence to return. Must be between 0 and the length of the input collection. If
        ///     <c>null</c> is specified, the size of the input collection is used.</param>
        /// <returns>
        ///     A collection containing all matching subsequences of the input <see cref="IEnumerable&lt;T&gt;"/>.</returns>
        public static IEnumerable<IEnumerable<T>> Subsequences<T>(this IEnumerable<T> source, int minLength = 0, int? maxLength = null)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            // Ensure that the source IEnumerable is evaluated only once
            var input = (source as IList<T>) ?? source.ToArray();

            if (minLength < 0 || minLength > input.Count)
                throw new ArgumentOutOfRangeException(nameof(minLength), "minLength must be between 0 and the size of the collection.");
            if (maxLength < 0 || maxLength > input.Count)
                throw new ArgumentOutOfRangeException(nameof(maxLength), "maxLength must be between 0 and the size of the collection.");

            IEnumerable<List<int>> subsequences(int range, int minLen, int maxLen)
            {
                var results = new List<List<int>>();
                if (minLen <= 0 && range == 0)
                    yield return new List<int>();
                else if (range > 0 && maxLen > 0)
                {
                    foreach (var list in subsequences(range - 1, minLen - 1, maxLen))
                    {
                        if (list.Count >= minLen)
                        {
                            if (list.Count < maxLen)
                            {
                                var list2 = list.ToList();
                                list2.Add(range - 1);
                                yield return list2;
                            }
                            yield return list;
                        }
                        else if (list.Count < maxLen)
                        {
                            list.Add(range - 1);
                            yield return list;
                        }
                    }
                }
            }
            return subsequences(input.Count, minLength, maxLength ?? input.Count).Select(ssq => ssq.Select(ix => input[ix]));
        }

        /// <summary>
        ///     Returns the first element of a sequence, or <c>null</c> if the sequence contains no elements.</summary>
        /// <typeparam name="T">
        ///     The type of the elements of <paramref name="source"/>.</typeparam>
        /// <param name="source">
        ///     The <see cref="IEnumerable&lt;T&gt;"/> to return the first element of.</param>
        /// <returns>
        ///     <c>null</c> if <paramref name="source"/> is empty; otherwise, the first element in <paramref name="source"/>.</returns>
        public static T? FirstOrNull<T>(this IEnumerable<T> source) where T : struct
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            using (var e = source.GetEnumerator())
            {
                if (e.MoveNext())
                    return e.Current;
                return null;
            }
        }

        /// <summary>
        ///     Returns the first element of a sequence that satisfies a given predicate, or <c>null</c> if the sequence
        ///     contains no elements.</summary>
        /// <typeparam name="T">
        ///     The type of the elements of <paramref name="source"/>.</typeparam>
        /// <param name="source">
        ///     The <see cref="IEnumerable&lt;T&gt;"/> to return the first element of.</param>
        /// <param name="predicate">
        ///     Only consider elements that satisfy this predicate.</param>
        /// <returns>
        ///     <c>null</c> if <paramref name="source"/> is empty; otherwise, the first element in <paramref name="source"/>.</returns>
        public static T? FirstOrNull<T>(this IEnumerable<T> source, Func<T, bool> predicate) where T : struct
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));
            using (var e = source.GetEnumerator())
            {
                while (e.MoveNext())
                    if (predicate(e.Current))
                        return e.Current;
                return null;
            }
        }

        /// <summary>
        ///     Returns the first element of a sequence, or a default value if the sequence contains no elements.</summary>
        /// <typeparam name="T">
        ///     The type of the elements of <paramref name="source"/>.</typeparam>
        /// <param name="source">
        ///     The <see cref="IEnumerable&lt;T&gt;"/> to return the first element of.</param>
        /// <param name="default">
        ///     The default value to return if the sequence contains no elements.</param>
        /// <returns>
        ///     <paramref name="default"/> if <paramref name="source"/> is empty; otherwise, the first element in <paramref
        ///     name="source"/>.</returns>
        public static T FirstOrDefault<T>(this IEnumerable<T> source, T @default)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            using (var e = source.GetEnumerator())
            {
                if (e.MoveNext())
                    return e.Current;
                return @default;
            }
        }

        /// <summary>
        ///     Returns the first element of a sequence, or a default value if the sequence contains no elements.</summary>
        /// <typeparam name="T">
        ///     The type of the elements of <paramref name="source"/>.</typeparam>
        /// <param name="source">
        ///     The <see cref="IEnumerable&lt;T&gt;"/> to return the first element of.</param>
        /// <param name="predicate">
        ///     A function to test each element for a condition.</param>
        /// <param name="default">
        ///     The default value to return if the sequence contains no elements.</param>
        /// <returns>
        ///     <paramref name="default"/> if <paramref name="source"/> is empty or if no element passes the test specified by
        ///     <paramref name="predicate"/>; otherwise, the first element in <paramref name="source"/> that passes the test
        ///     specified by <paramref name="predicate"/>.</returns>
        public static T FirstOrDefault<T>(this IEnumerable<T> source, Func<T, bool> predicate, T @default)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));
            using (var e = source.GetEnumerator())
            {
                while (e.MoveNext())
                    if (predicate(e.Current))
                        return e.Current;
                return @default;
            }
        }

        /// <summary>
        ///     Returns the first element of a sequence, or a default value if the sequence contains no elements.</summary>
        /// <typeparam name="TSource">
        ///     The type of the elements of <paramref name="source"/>.</typeparam>
        /// <typeparam name="TResult">
        ///     The type of the resulting value.</typeparam>
        /// <param name="source">
        ///     The <see cref="IEnumerable&lt;T&gt;"/> to return the first element of.</param>
        /// <param name="predicate">
        ///     A function to test each element for a condition.</param>
        /// <param name="resultSelector">
        ///     A function to transform the first element into the result value. Will only be called if the sequence contains
        ///     an element that passes the test specified by <paramref name="predicate"/>.</param>
        /// <param name="default">
        ///     The default value to return if the sequence contains no elements.</param>
        /// <returns>
        ///     <paramref name="default"/> if <paramref name="source"/> is empty or if no element passes the test specified by
        ///     <paramref name="predicate"/>; otherwise, the transformed first element in <paramref name="source"/> that
        ///     passes the test specified by <paramref name="predicate"/>.</returns>
        public static TResult FirstOrDefault<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, bool> predicate, Func<TSource, TResult> resultSelector, TResult @default)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));
            if (resultSelector == null)
                throw new ArgumentNullException(nameof(resultSelector));
            using (var e = source.GetEnumerator())
            {
                while (e.MoveNext())
                    if (predicate(e.Current))
                        return resultSelector(e.Current);
                return @default;
            }
        }

        /// <summary>
        ///     Returns the index of the first element in this <paramref name="source"/> satisfying the specified <paramref
        ///     name="predicate"/>. If no such elements are found, returns <c>-1</c>.</summary>
        public static int IndexOf<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));
            int index = 0;
            foreach (var v in source)
            {
                if (predicate(v))
                    return index;
                index++;
            }
            return -1;
        }

        /// <summary>
        ///     Returns the index of the first element in this <paramref name="source"/> satisfying the specified <paramref
        ///     name="predicate"/>. The second parameter receives the index of each element. If no such elements are found,
        ///     returns <c>-1</c>.</summary>
        public static int IndexOf<T>(this IEnumerable<T> source, Func<T, int, bool> predicate)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));
            int index = 0;
            foreach (var v in source)
            {
                if (predicate(v, index))
                    return index;
                index++;
            }
            return -1;
        }

        /// <summary>
        ///     Returns the index of the first element in this <paramref name="source"/> satisfying the specified <paramref
        ///     name="predicate"/>, starting at the specified <paramref name="startIndex"/>. If no such elements are found,
        ///     returns <c>-1</c>.</summary>
        public static int IndexOf<T>(this IEnumerable<T> source, Func<T, bool> predicate, int startIndex)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));
            if (startIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(startIndex), "startIndex cannot be negative.");
            int index = 0;
            foreach (var v in source)
            {
                if (predicate(v) && index >= startIndex)
                    return index;
                index++;
            }
            return -1;
        }

        /// <summary>
        ///     Returns the index of the last element in this <paramref name="source"/> satisfying the specified <paramref
        ///     name="predicate"/>. If no such elements are found, returns <c>-1</c>.</summary>
        /// <remarks>
        ///     This method is optimised for the case in which the input sequence is a list or array. In all other cases, the
        ///     collection is evaluated completely before this method returns.</remarks>
        public static int LastIndexOf<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            var list = source as IList<T>;
            if (list != null)
            {
                var i = list.Count - 1;
                while (i >= 0 && !predicate(list[i]))
                    i--;
                return i;
            }

            int index = 0;
            int lastIndex = -1;
            foreach (var v in source)
            {
                if (predicate(v))
                    lastIndex = index;
                index++;
            }
            return lastIndex;
        }

        /// <summary>
        ///     Returns the index of the first element in this <paramref name="source"/> that is equal to the specified
        ///     <paramref name="element"/> as determined by the specified <paramref name="comparer"/>. If no such elements are
        ///     found, returns <c>-1</c>.</summary>
        public static int IndexOf<T>(this IEnumerable<T> source, T element, IEqualityComparer<T> comparer = null)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (comparer == null)
                comparer = EqualityComparer<T>.Default;
            int index = 0;
            foreach (var v in source)
            {
                if (comparer.Equals(v, element))
                    return index;
                index++;
            }
            return -1;
        }

        /// <summary>
        ///     Returns the minimum resulting value in a sequence, or a default value if the sequence is empty.</summary>
        /// <typeparam name="TSource">
        ///     The type of the elements of <paramref name="source"/>.</typeparam>
        /// <param name="source">
        ///     A sequence of values to determine the minimum value of.</param>
        /// <param name="default">
        ///     A default value to return in case the sequence is empty.</param>
        /// <returns>
        ///     The minimum value in the sequence, or the specified default value if the sequence is empty.</returns>
        public static TSource MinOrDefault<TSource>(this IEnumerable<TSource> source, TSource @default = default)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            var (result, found) = minMax(source, min: true);
            return found ? result : @default;
        }

        /// <summary>
        ///     Invokes a selector on each element of a collection and returns the minimum resulting value, or a default value
        ///     if the sequence is empty.</summary>
        /// <typeparam name="TSource">
        ///     The type of the elements of <paramref name="source"/>.</typeparam>
        /// <typeparam name="TResult">
        ///     The type of the value returned by <paramref name="selector"/>.</typeparam>
        /// <param name="source">
        ///     A sequence of values to determine the minimum value of.</param>
        /// <param name="selector">
        ///     A transform function to apply to each element.</param>
        /// <param name="default">
        ///     A default value to return in case the sequence is empty.</param>
        /// <returns>
        ///     The minimum value in the sequence, or the specified default value if the sequence is empty.</returns>
        public static TResult MinOrDefault<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector, TResult @default = default)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (selector == null)
                throw new ArgumentNullException(nameof(selector));
            var (result, found) = minMax(source.Select(selector), min: true);
            return found ? result : @default;
        }

        /// <summary>
        ///     Returns the maximum resulting value in a sequence, or a default value if the sequence is empty.</summary>
        /// <typeparam name="TSource">
        ///     The type of the elements of <paramref name="source"/>.</typeparam>
        /// <param name="source">
        ///     A sequence of values to determine the maximum value of.</param>
        /// <param name="default">
        ///     A default value to return in case the sequence is empty.</param>
        /// <returns>
        ///     The maximum value in the sequence, or the specified default value if the sequence is empty.</returns>
        public static TSource MaxOrDefault<TSource>(this IEnumerable<TSource> source, TSource @default = default)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            var (result, found) = minMax(source, min: false);
            return found ? result : @default;
        }

        /// <summary>
        ///     Invokes a selector on each element of a collection and returns the maximum resulting value, or a default value
        ///     if the sequence is empty.</summary>
        /// <typeparam name="TSource">
        ///     The type of the elements of <paramref name="source"/>.</typeparam>
        /// <typeparam name="TResult">
        ///     The type of the value returned by <paramref name="selector"/>.</typeparam>
        /// <param name="source">
        ///     A sequence of values to determine the maximum value of.</param>
        /// <param name="selector">
        ///     A transform function to apply to each element.</param>
        /// <param name="default">
        ///     A default value to return in case the sequence is empty.</param>
        /// <returns>
        ///     The maximum value in the sequence, or the specified default value if the sequence is empty.</returns>
        public static TResult MaxOrDefault<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector, TResult @default = default)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (selector == null)
                throw new ArgumentNullException(nameof(selector));
            var (result, found) = minMax(source.Select(selector), min: false);
            return found ? result : @default;
        }

        /// <summary>
        ///     Returns the minimum resulting value in a sequence, or <c>null</c> if the sequence is empty.</summary>
        /// <typeparam name="TSource">
        ///     The type of the elements of <paramref name="source"/>.</typeparam>
        /// <param name="source">
        ///     A sequence of values to determine the minimum value of.</param>
        /// <returns>
        ///     The minimum value in the sequence, or <c>null</c> if the sequence is empty.</returns>
        public static TSource? MinOrNull<TSource>(this IEnumerable<TSource> source) where TSource : struct
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            var (result, found) = minMax(source, min: true);
            return found ? result : (TSource?) null;
        }

        /// <summary>
        ///     Invokes a selector on each element of a collection and returns the minimum resulting value, or <c>null</c> if
        ///     the sequence is empty.</summary>
        /// <typeparam name="TSource">
        ///     The type of the elements of <paramref name="source"/>.</typeparam>
        /// <typeparam name="TResult">
        ///     The type of the value returned by <paramref name="selector"/>.</typeparam>
        /// <param name="source">
        ///     A sequence of values to determine the minimum value of.</param>
        /// <param name="selector">
        ///     A transform function to apply to each element.</param>
        /// <returns>
        ///     The minimum value in the sequence, or <c>null</c> if the sequence is empty.</returns>
        public static TResult? MinOrNull<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector) where TResult : struct
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (selector == null)
                throw new ArgumentNullException(nameof(selector));
            var (result, found) = minMax(source.Select(selector), min: true);
            return found ? result : (TResult?) null;
        }

        /// <summary>
        ///     Returns the maximum resulting value in a sequence, or <c>null</c> if the sequence is empty.</summary>
        /// <typeparam name="TSource">
        ///     The type of the elements of <paramref name="source"/>.</typeparam>
        /// <param name="source">
        ///     A sequence of values to determine the maximum value of.</param>
        /// <returns>
        ///     The maximum value in the sequence, or <c>null</c> if the sequence is empty.</returns>
        public static TSource? MaxOrNull<TSource>(this IEnumerable<TSource> source) where TSource : struct
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            var (result, found) = minMax(source, min: false);
            return found ? result : (TSource?) null;
        }

        /// <summary>
        ///     Invokes a selector on each element of a collection and returns the maximum resulting value, or <c>null</c> if
        ///     the sequence is empty.</summary>
        /// <typeparam name="TSource">
        ///     The type of the elements of <paramref name="source"/>.</typeparam>
        /// <typeparam name="TResult">
        ///     The type of the value returned by <paramref name="selector"/>.</typeparam>
        /// <param name="source">
        ///     A sequence of values to determine the maximum value of.</param>
        /// <param name="selector">
        ///     A transform function to apply to each element.</param>
        /// <returns>
        ///     The maximum value in the sequence, or <c>null</c> if the sequence is empty.</returns>
        public static TResult? MaxOrNull<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector) where TResult : struct
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (selector == null)
                throw new ArgumentNullException(nameof(selector));
            var (result, found) = minMax(source.Select(selector), min: false);
            return found ? result : (TResult?) null;
        }

        private static (T result, bool found) minMax<T>(IEnumerable<T> source, bool min)
        {
            var cmp = Comparer<T>.Default;
            var curBest = default(T);
            var haveBest = false;
            foreach (var elem in source)
            {
                if (!haveBest || (min ? cmp.Compare(elem, curBest) < 0 : cmp.Compare(elem, curBest) > 0))
                {
                    curBest = elem;
                    haveBest = true;
                }
            }
            return (curBest, haveBest);
        }

        /// <summary>
        ///     Returns the first element from the input sequence for which the value selector returns the smallest value.</summary>
        /// <exception cref="InvalidOperationException">
        ///     The input collection is empty.</exception>
        public static T MinElement<T, TValue>(this IEnumerable<T> source, Func<T, TValue> valueSelector) where TValue : IComparable<TValue> =>
            minMaxElement(source, valueSelector, min: true, doThrow: true).Value.minMaxElem;

        /// <summary>
        ///     Returns the first element from the input sequence for which the value selector returns the smallest value, or
        ///     a default value if the collection is empty.</summary>
        public static T MinElementOrDefault<T, TValue>(this IEnumerable<T> source, Func<T, TValue> valueSelector, T defaultValue = default) where TValue : IComparable<TValue>
        {
            var tup = minMaxElement(source, valueSelector, min: true, doThrow: false);
            return tup == null ? defaultValue : tup.Value.minMaxElem;
        }

        /// <summary>
        ///     Returns the first element from the input sequence for which the value selector returns the largest value.</summary>
        /// <exception cref="InvalidOperationException">
        ///     The input collection is empty.</exception>
        public static T MaxElement<T, TValue>(this IEnumerable<T> source, Func<T, TValue> valueSelector) where TValue : IComparable<TValue> =>
            minMaxElement(source, valueSelector, min: false, doThrow: true).Value.minMaxElem;

        /// <summary>
        ///     Returns the first element from the input sequence for which the value selector returns the largest value, or a
        ///     default value if the collection is empty.</summary>
        public static T MaxElementOrDefault<T, TValue>(this IEnumerable<T> source, Func<T, TValue> valueSelector, T defaultValue = default(T)) where TValue : IComparable<TValue>
        {
            var tup = minMaxElement(source, valueSelector, min: false, doThrow: false);
            return tup == null ? defaultValue : tup.Value.minMaxElem;
        }

        /// <summary>
        ///     Returns the index of the first element from the input sequence for which the value selector returns the
        ///     smallest value.</summary>
        /// <exception cref="InvalidOperationException">
        ///     The input collection is empty.</exception>
        public static int MinIndex<T, TValue>(this IEnumerable<T> source, Func<T, TValue> valueSelector) where TValue : IComparable<TValue> =>
            minMaxElement(source, valueSelector, min: true, doThrow: true).Value.minMaxIndex;

        /// <summary>
        ///     Returns the index of the first element from the input sequence for which the value selector returns the
        ///     smallest value, or <c>null</c> if the collection is empty.</summary>
        public static int? MinIndexOrNull<T, TValue>(this IEnumerable<T> source, Func<T, TValue> valueSelector) where TValue : IComparable<TValue> =>
            minMaxElement(source, valueSelector, min: true, doThrow: false)?.minMaxIndex;

        /// <summary>
        ///     Returns the index of the first element from the input sequence for which the value selector returns the
        ///     largest value.</summary>
        /// <exception cref="InvalidOperationException">
        ///     The input collection is empty.</exception>
        public static int MaxIndex<T, TValue>(this IEnumerable<T> source, Func<T, TValue> valueSelector) where TValue : IComparable<TValue> =>
            minMaxElement(source, valueSelector, min: false, doThrow: true).Value.minMaxIndex;

        /// <summary>
        ///     Returns the index of the first element from the input sequence for which the value selector returns the
        ///     largest value, or a default value if the collection is empty.</summary>
        public static int? MaxIndexOrNull<T, TValue>(this IEnumerable<T> source, Func<T, TValue> valueSelector) where TValue : IComparable<TValue> =>
            minMaxElement(source, valueSelector, min: false, doThrow: false)?.minMaxIndex;

        private static (int minMaxIndex, T minMaxElem)? minMaxElement<T, TValue>(IEnumerable<T> source, Func<T, TValue> valueSelector, bool min, bool doThrow) where TValue : IComparable<TValue>
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (valueSelector == null)
                throw new ArgumentNullException(nameof(valueSelector));

            using (var enumerator = source.GetEnumerator())
            {
                if (!enumerator.MoveNext())
                {
                    if (doThrow)
                        throw new InvalidOperationException("source contains no elements.");
                    return null;
                }
                var minMaxElem = enumerator.Current;
                var minMaxValue = valueSelector(minMaxElem);
                var minMaxIndex = 0;
                var curIndex = 0;
                while (enumerator.MoveNext())
                {
                    curIndex++;
                    var value = valueSelector(enumerator.Current);
                    if (min ? (value.CompareTo(minMaxValue) < 0) : (value.CompareTo(minMaxValue) > 0))
                    {
                        minMaxValue = value;
                        minMaxElem = enumerator.Current;
                        minMaxIndex = curIndex;
                    }
                }
                return (minMaxIndex, minMaxElem);
            }
        }

        /// <summary>
        ///     Enumerates the items of this collection, skipping the last <paramref name="count"/> items. Note that the
        ///     memory usage of this method is proportional to <paramref name="count"/>, but the source collection is only
        ///     enumerated once, and in a lazy fashion. Also, enumerating the first item will take longer than enumerating
        ///     subsequent items.</summary>
        /// <param name="source">
        ///     Source collection.</param>
        /// <param name="count">
        ///     Number of items to skip from the end of the collection.</param>
        /// <param name="throwIfNotEnough">
        ///     If <c>true</c>, the enumerator throws at the end of the enumeration if the source collection contained fewer
        ///     than <paramref name="count"/> elements.</param>
        public static IEnumerable<T> SkipLast<T>(this IEnumerable<T> source, int count, bool throwIfNotEnough = false)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), "count cannot be negative.");
            if (count == 0)
                return source;

            if (source is ICollection<T> collection)
            {
                if (throwIfNotEnough && collection.Count < count)
                    throw new InvalidOperationException("The collection does not contain enough elements.");
                return collection.Take(Math.Max(0, collection.Count - count));
            }

            IEnumerable<T> skipLastIterator()
            {
                var queue = new T[count];
                int headtail = 0; // tail while we're still collecting, both head & tail afterwards because the queue becomes completely full
                int collected = 0;

                foreach (var item in source)
                {
                    if (collected < count)
                    {
                        queue[headtail] = item;
                        headtail++;
                        collected++;
                    }
                    else
                    {
                        if (headtail == count)
                            headtail = 0;
                        yield return queue[headtail];
                        queue[headtail] = item;
                        headtail++;
                    }
                }

                if (throwIfNotEnough && collected < count)
                    throw new InvalidOperationException("The collection does not contain enough elements.");
            }
            return skipLastIterator();
        }

        /// <summary>
        ///     Returns a collection containing only the last <paramref name="count"/> items of the input collection. This
        ///     method enumerates the entire collection to the end once before returning. Note also that the memory usage of
        ///     this method is proportional to <paramref name="count"/>.</summary>
        public static IEnumerable<T> TakeLast<T>(this IEnumerable<T> source, int count)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), "count cannot be negative.");
            if (count == 0)
                return Enumerable.Empty<T>();

            if (source is IList<T> list)
            {
                // Make this a local iterator-block function so that list.Count is only evaluated when enumeration begins
                IEnumerable<T> takeLastFromList()
                {
                    for (int i = Math.Max(0, list.Count - count); i < list.Count; i++)
                        yield return list[i];
                }
                return takeLastFromList();
            }
            else if (source is ICollection<T> collection)
            {
                // Make this a local iterator-block function so that collection.Count is only evaluated when enumeration begins
                IEnumerable<T> takeLastFromCollection()
                {
                    foreach (var elem in collection.Skip(Math.Max(0, collection.Count - count)))
                        yield return elem;
                }
                return takeLastFromCollection();
            }
            else
            {
                IEnumerable<T> takeLast()
                {
                    var queue = new Queue<T>(count + 1);
                    foreach (var item in source)
                    {
                        if (queue.Count == count)
                            queue.Dequeue();
                        queue.Enqueue(item);
                    }
                    foreach (var item in queue)
                        yield return item;
                }
                return takeLast();
            }
        }

        /// <summary>Returns true if and only if the input collection begins with the specified collection.</summary>
        public static bool StartsWith<T>(this IEnumerable<T> source, IEnumerable<T> sequence)
        {
            return StartsWith<T>(source, sequence, EqualityComparer<T>.Default);
        }

        /// <summary>Returns true if and only if the input collection begins with the specified collection.</summary>
        public static bool StartsWith<T>(this IEnumerable<T> source, IEnumerable<T> sequence, IEqualityComparer<T> comparer)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (sequence == null)
                throw new ArgumentNullException(nameof(sequence));
            if (comparer == null)
                throw new ArgumentNullException(nameof(comparer));

            using (var sourceEnum = source.GetEnumerator())
            using (var seqEnum = sequence.GetEnumerator())
            {
                while (true)
                {
                    if (!seqEnum.MoveNext())
                        return true;
                    if (!sourceEnum.MoveNext())
                        return false;
                    if (!comparer.Equals(sourceEnum.Current, seqEnum.Current))
                        return false;
                }
            }
        }

        /// <summary>Creates a <see cref="Queue{T}"/> from an enumerable collection.</summary>
        public static Queue<T> ToQueue<T>(this IEnumerable<T> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            return new Queue<T>(source);
        }

        /// <summary>Creates a <see cref="Stack{T}"/> from an enumerable collection.</summary>
        public static Stack<T> ToStack<T>(this IEnumerable<T> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            return new Stack<T>(source);
        }

        /// <summary>Creates a <see cref="HashSet{T}"/> from an enumerable collection.</summary>
        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source, IEqualityComparer<T> comparer = null)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            return comparer == null ? new HashSet<T>(source) : new HashSet<T>(source, comparer);
        }

        /// <summary>
        ///     Creates a two-level dictionary from an enumerable collection according to two specified key selector functions
        ///     and optional key comparers.</summary>
        /// <typeparam name="TSource">
        ///     The type of the elements of <paramref name="source"/>.</typeparam>
        /// <typeparam name="TKey1">
        ///     The type of the keys returned by <paramref name="key1Selector"/>.</typeparam>
        /// <typeparam name="TKey2">
        ///     The type of the keys returned by <paramref name="key2Selector"/>.</typeparam>
        /// <param name="source">
        ///     Source collection to create a dictionary from.</param>
        /// <param name="key1Selector">
        ///     A function to extract the first-level key from each element.</param>
        /// <param name="key2Selector">
        ///     A function to extract the second-level key from each element.</param>
        /// <param name="comparer1">
        ///     An equality comparer to compare the first-level keys.</param>
        /// <param name="comparer2">
        ///     An equality comparer to compare the second-level keys.</param>
        public static Dictionary<TKey1, Dictionary<TKey2, TSource>> ToDictionary2<TSource, TKey1, TKey2>(this IEnumerable<TSource> source,
            Func<TSource, TKey1> key1Selector, Func<TSource, TKey2> key2Selector, IEqualityComparer<TKey1> comparer1 = null, IEqualityComparer<TKey2> comparer2 = null)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (key1Selector == null)
                throw new ArgumentNullException(nameof(key1Selector));
            if (key2Selector == null)
                throw new ArgumentNullException(nameof(key2Selector));

            var newDic = new Dictionary<TKey1, Dictionary<TKey2, TSource>>(comparer1 ?? EqualityComparer<TKey1>.Default);
            foreach (var elem in source)
                newDic.AddSafe(key1Selector(elem), key2Selector(elem), elem, comparer2);
            return newDic;
        }

        /// <summary>
        ///     Creates a two-level dictionary from an enumerable collection according to two specified key selector functions
        ///     and optional key comparers.</summary>
        /// <typeparam name="TSource">
        ///     The type of the elements of <paramref name="source"/>.</typeparam>
        /// <typeparam name="TKey1">
        ///     The type of the keys returned by <paramref name="key1Selector"/>.</typeparam>
        /// <typeparam name="TKey2">
        ///     The type of the keys returned by <paramref name="key2Selector"/>.</typeparam>
        /// <typeparam name="TValue">
        ///     The type of the value returned by <paramref name="elementSelector"/>.</typeparam>
        /// <param name="source">
        ///     Source collection to create a dictionary from.</param>
        /// <param name="key1Selector">
        ///     A function to extract the first-level key from each element.</param>
        /// <param name="key2Selector">
        ///     A function to extract the second-level key from each element.</param>
        /// <param name="elementSelector">
        ///     A transform function to produce a result element value from each element.</param>
        /// <param name="comparer1">
        ///     An equality comparer to compare the first-level keys.</param>
        /// <param name="comparer2">
        ///     An equality comparer to compare the second-level keys.</param>
        public static Dictionary<TKey1, Dictionary<TKey2, TValue>> ToDictionary2<TSource, TKey1, TKey2, TValue>(this IEnumerable<TSource> source,
            Func<TSource, TKey1> key1Selector, Func<TSource, TKey2> key2Selector, Func<TSource, TValue> elementSelector,
            IEqualityComparer<TKey1> comparer1 = null, IEqualityComparer<TKey2> comparer2 = null)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (key1Selector == null)
                throw new ArgumentNullException(nameof(key1Selector));
            if (key2Selector == null)
                throw new ArgumentNullException(nameof(key2Selector));
            if (elementSelector == null)
                throw new ArgumentNullException(nameof(elementSelector));

            var newDic = new Dictionary<TKey1, Dictionary<TKey2, TValue>>(comparer1 ?? EqualityComparer<TKey1>.Default);
            foreach (var elem in source)
                newDic.AddSafe(key1Selector(elem), key2Selector(elem), elementSelector(elem), comparer2);
            return newDic;
        }

        /// <summary>
        ///     Returns a collection of integers containing the indexes at which the elements of the source collection match
        ///     the given predicate.</summary>
        /// <typeparam name="T">
        ///     The type of elements in the collection.</typeparam>
        /// <param name="source">
        ///     The source collection whose elements are tested using <paramref name="predicate"/>.</param>
        /// <param name="predicate">
        ///     The predicate against which the elements of <paramref name="source"/> are tested.</param>
        /// <returns>
        ///     A collection containing the zero-based indexes of all the matching elements, in increasing order.</returns>
        public static IEnumerable<int> SelectIndexWhere<T>(this IEnumerable<T> source, Predicate<T> predicate)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            IEnumerable<int> selectIndexWhereIterator()
            {
                int i = 0;
                using (var e = source.GetEnumerator())
                {
                    while (e.MoveNext())
                    {
                        if (predicate(e.Current))
                            yield return i;
                        i++;
                    }
                }
            }
            return selectIndexWhereIterator();
        }

        /// <summary>
        ///     Transforms every element of an input collection using two selector functions and returns a collection
        ///     containing all the results.</summary>
        /// <typeparam name="TSource">
        ///     Type of the elements in the source collection.</typeparam>
        /// <typeparam name="TResult">
        ///     Type of the results of the selector functions.</typeparam>
        /// <param name="source">
        ///     Input collection to transform.</param>
        /// <param name="selector1">
        ///     First selector function.</param>
        /// <param name="selector2">
        ///     Second selector function.</param>
        /// <returns>
        ///     A collection containing the transformed elements from both selectors, thus containing twice as many elements
        ///     as the original collection.</returns>
        public static IEnumerable<TResult> SelectTwo<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector1, Func<TSource, TResult> selector2)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (selector1 == null)
                throw new ArgumentNullException(nameof(selector1));
            if (selector2 == null)
                throw new ArgumentNullException(nameof(selector2));

            IEnumerable<TResult> selectTwoIterator()
            {
                foreach (var elem in source)
                {
                    yield return selector1(elem);
                    yield return selector2(elem);
                }
            }
            return selectTwoIterator();
        }

        /// <summary>Returns the original collection but with every value cast to their nullable equivalent.</summary>
        public static IEnumerable<TResult?> SelectNullable<TResult>(this IEnumerable<TResult> source) where TResult : struct => source.Select(val => (TResult?) val);

        /// <summary>
        ///     Turns all elements in the enumerable to strings and joins them using the specified <paramref
        ///     name="separator"/> and the specified <paramref name="prefix"/> and <paramref name="suffix"/> for each string.</summary>
        /// <param name="values">
        ///     The sequence of elements to join into a string.</param>
        /// <param name="separator">
        ///     Optionally, a separator to insert between each element and the next.</param>
        /// <param name="prefix">
        ///     Optionally, a string to insert in front of each element.</param>
        /// <param name="suffix">
        ///     Optionally, a string to insert after each element.</param>
        /// <param name="lastSeparator">
        ///     Optionally, a separator to use between the second-to-last and the last element.</param>
        /// <example>
        ///     <code>
        ///         // Returns "[Paris], [London], [Tokyo]"
        ///         (new[] { "Paris", "London", "Tokyo" }).JoinString(", ", "[", "]")
        ///         
        ///         // Returns "[Paris], [London] and [Tokyo]"
        ///         (new[] { "Paris", "London", "Tokyo" }).JoinString(", ", "[", "]", " and ");</code></example>
        public static string JoinString<T>(this IEnumerable<T> values, string separator = null, string prefix = null, string suffix = null, string lastSeparator = null)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));
            if (lastSeparator == null)
                lastSeparator = separator;

            using (var enumerator = values.GetEnumerator())
            {
                if (!enumerator.MoveNext())
                    return "";

                // Optimise the case where there is only one element
                var one = enumerator.Current;
                if (!enumerator.MoveNext())
                    return prefix + one + suffix;

                // Optimise the case where there are only two elements
                var two = enumerator.Current;
                if (!enumerator.MoveNext())
                {
                    // Optimise the (common) case where there is no prefix/suffix; this prevents an array allocation when calling string.Concat()
                    if (prefix == null && suffix == null)
                        return one + lastSeparator + two;
                    return prefix + one + suffix + lastSeparator + prefix + two + suffix;
                }

                StringBuilder sb = new StringBuilder()
                    .Append(prefix).Append(one).Append(suffix).Append(separator)
                    .Append(prefix).Append(two).Append(suffix);
                var prev = enumerator.Current;
                while (enumerator.MoveNext())
                {
                    sb.Append(separator).Append(prefix).Append(prev).Append(suffix);
                    prev = enumerator.Current;
                }
                sb.Append(lastSeparator).Append(prefix).Append(prev).Append(suffix);
                return sb.ToString();
            }
        }

        /// <summary>
        ///     Inserts the specified item in between each element in the input collection.</summary>
        /// <param name="source">
        ///     The input collection.</param>
        /// <param name="extraElement">
        ///     The element to insert between each consecutive pair of elements in the input collection.</param>
        /// <returns>
        ///     A collection containing the original collection with the extra element inserted. For example, new[] { 1, 2, 3
        ///     }.InsertBetween(0) returns { 1, 0, 2, 0, 3 }.</returns>
        public static IEnumerable<T> InsertBetween<T>(this IEnumerable<T> source, T extraElement)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            return source.SelectMany(val => new[] { extraElement, val }).Skip(1);
        }

        /// <summary>
        ///     Inserts the <paramref name="comma"/> item in between each element in the input collection except between the
        ///     second-last and last, where it inserts <paramref name="and"/> instead.</summary>
        /// <param name="source">
        ///     The input collection.</param>
        /// <param name="comma">
        ///     The element to insert between each consecutive pair of elements in the input collection except between the
        ///     second-last and last.</param>
        /// <param name="and">
        ///     The element to insert between the second-last and last element of the input collection.</param>
        /// <returns>
        ///     A collection containing the original collection with the extra element inserted. For example, new[] { "a",
        ///     "b", "c" }.InsertBetweenWithAnd(", ", " and ") returns { "a", ", ", "b", " and ", "c" }.</returns>
        public static IEnumerable<T> InsertBetweenWithAnd<T>(this IEnumerable<T> source, T comma, T and)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            IEnumerable<T> insertBetweenWithAndIterator()
            {
                using (var enumerator = source.GetEnumerator())
                {
                    if (!enumerator.MoveNext())
                        yield break;
                    yield return enumerator.Current;
                    if (!enumerator.MoveNext())
                        yield break;

                    var prev = enumerator.Current;
                    while (enumerator.MoveNext())
                    {
                        yield return comma;
                        yield return prev;
                        prev = enumerator.Current;
                    }
                    yield return and;
                    yield return prev;
                }
            }

            return insertBetweenWithAndIterator();
        }

        /// <summary>Determines whether this sequence comprises the values provided in the specified order.</summary>
        public static bool SequenceEqual<T>(this IEnumerable<T> sequence1, params T[] sequence2)
        {
            return Enumerable.SequenceEqual(sequence1, sequence2);
        }

        /// <summary>Determines whether all the input sequences are equal according to SequenceEquals.</summary>
        public static bool AllSequencesEqual<T>(this IEnumerable<IEnumerable<T>> sources)
        {
            using (var e = sources.GetEnumerator())
            {
                if (!e.MoveNext())
                    return true;
                var firstSequence = e.Current;
                while (e.MoveNext())
                    if (!firstSequence.SequenceEqual(e.Current))
                        return false;
                return true;
            }
        }

        /// <summary>
        ///     Splits a collection into chunks of equal size. The last chunk may be smaller than <paramref
        ///     name="chunkSize"/>, but all chunks, if any, will contain at least one item.</summary>
        public static IEnumerable<IEnumerable<T>> Split<T>(this IEnumerable<T> source, int chunkSize)
        {
            if (chunkSize <= 0)
                throw new ArgumentException("chunkSize must be greater than zero.", nameof(chunkSize));

            IEnumerable<IEnumerable<T>> splitIterator()
            {
                var list = new List<T>(chunkSize);
                foreach (var item in source)
                {
                    list.Add(item);
                    if (list.Count == chunkSize)
                    {
                        yield return list;
                        list = new List<T>(chunkSize);
                    }
                }
                if (list.Count > 0)
                    yield return list;
            }
            return splitIterator();
        }

        /// <summary>
        ///     Accumulates consecutive equal elements.</summary>
        /// <typeparam name="TItem">
        ///     The type of items in the input sequence.</typeparam>
        /// <param name="source">
        ///     The input sequence from which to accumulate groups of consecutive elements.</param>
        /// <param name="itemEquality">
        ///     An optional function to determine equality of items.</param>
        /// <returns>
        ///     A collection containing each sequence of consecutive equal elements.</returns>
        public static IEnumerable<ConsecutiveGroup<TItem, TItem>> GroupConsecutive<TItem>(this IEnumerable<TItem> source, Func<TItem, TItem, bool> itemEquality) =>
            GroupConsecutiveBy(source, x => x, new CustomEqualityComparer<TItem>(itemEquality));

        /// <summary>
        ///     Accumulates consecutive equal elements.</summary>
        /// <typeparam name="TItem">
        ///     The type of items in the input sequence.</typeparam>
        /// <param name="source">
        ///     The input sequence from which to accumulate groups of consecutive elements.</param>
        /// <param name="itemComparer">
        ///     An optional equality comparer to determine item equality by.</param>
        /// <returns>
        ///     A collection containing each sequence of consecutive equal elements.</returns>
        public static IEnumerable<ConsecutiveGroup<TItem, TItem>> GroupConsecutive<TItem>(this IEnumerable<TItem> source, IEqualityComparer<TItem> itemComparer = null) =>
            GroupConsecutiveBy(source, x => x, itemComparer);

        /// <summary>
        ///     Accumulates consecutive elements that are equal when processed by a selector.</summary>
        /// <typeparam name="TItem">
        ///     The type of items in the input sequence.</typeparam>
        /// <typeparam name="TKey">
        ///     The return type of the <paramref name="selector"/> function.</typeparam>
        /// <param name="source">
        ///     The input sequence from which to accumulate groups of consecutive elements.</param>
        /// <param name="selector">
        ///     A function to transform each item into a key which is compared for equality.</param>
        /// <param name="keyComparer">
        ///     An optional equality comparer for the keys returned by <paramref name="selector"/>.</param>
        /// <returns>
        ///     A collection containing each sequence of consecutive equal elements.</returns>
        public static IEnumerable<ConsecutiveGroup<TItem, TKey>> GroupConsecutiveBy<TItem, TKey>(this IEnumerable<TItem> source, Func<TItem, TKey> selector, IEqualityComparer<TKey> keyComparer = null)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (selector == null)
                throw new ArgumentNullException(nameof(selector));

            IEnumerable<ConsecutiveGroup<TItem, TKey>> groupConsecutiveIterator()
            {
                var any = false;
                var prevKey = default(TKey);
                var index = 0;
                var currentList = new List<TItem>();
                foreach (var elem in source)
                {
                    var key = selector(elem);
                    if (!any)
                        any = true;
                    else if (keyComparer != null ? !keyComparer.Equals(prevKey, key) : !Equals(prevKey, key))
                    {
                        yield return new ConsecutiveGroup<TItem, TKey>(index - currentList.Count, currentList, prevKey);
                        currentList = new List<TItem>();
                    }
                    currentList.Add(elem);
                    prevKey = key;
                    index++;
                }
                if (any)
                    yield return new ConsecutiveGroup<TItem, TKey>(index - currentList.Count, currentList, prevKey);
            }
            return groupConsecutiveIterator();
        }

        /// <summary>
        ///     Enumerates a chain of objects where each object refers to the next one. The chain starts with the specified
        ///     object and ends when null is encountered.</summary>
        /// <typeparam name="T">
        ///     Type of object to enumerate.</typeparam>
        /// <param name="obj">
        ///     Initial object.</param>
        /// <param name="next">
        ///     A function that returns the next object given the current one. If null is returned, enumeration will end.</param>
        public static IEnumerable<T> SelectChain<T>(this T obj, Func<T, T> next) where T : class
        {
            while (obj != null)
            {
                yield return obj;
                obj = next(obj);
            }
        }

        /// <summary>
        ///     Determines which element occurs the most often in the specified input sequence.</summary>
        /// <typeparam name="T">
        ///     Type of elements in the input sequence.</typeparam>
        /// <param name="source">
        ///     Sequence to find most common element in.</param>
        /// <param name="comparer">
        ///     Optional equality comparer to compare elements by.</param>
        /// <returns>
        ///     Of all elements that occur the most number of times, the one whose last instance occurs soonest in the
        ///     sequence.</returns>
        public static T MaxCountElement<T>(this IEnumerable<T> source, IEqualityComparer<T> comparer = null) => MaxCountElement(source, out _, comparer);

        /// <summary>
        ///     Determines which element occurs the most often in the specified input sequence, and how often.</summary>
        /// <typeparam name="T">
        ///     Type of elements in the input sequence.</typeparam>
        /// <param name="source">
        ///     Sequence to find most common element in.</param>
        /// <param name="count">
        ///     Receives the number of times the element occurred.</param>
        /// <param name="comparer">
        ///     Optional equality comparer to compare elements by.</param>
        /// <returns>
        ///     Of all elements that occur the most number of times, the one whose last instance occurs soonest in the
        ///     sequence.</returns>
        public static T MaxCountElement<T>(this IEnumerable<T> source, out int count, IEqualityComparer<T> comparer = null)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            var counts = new Dictionary<T, int>(comparer);
            var curMaxElement = default(T);
            count = 0;
            foreach (var elem in source)
            {
                var newCount = counts.IncSafe(elem);
                if (newCount > count)
                {
                    count = newCount;
                    curMaxElement = elem;
                }
            }
            if (count == 0)
                throw new InvalidOperationException("The specified collection contained no elements.");
            return curMaxElement;
        }

        /// <summary>Returns the sum of the values in the specified collection, truncated to a 32-bit integer.</summary>
        public static int SumUnchecked(this IEnumerable<int> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            unchecked
            {
                var sum = 0;
                foreach (var value in source)
                    sum += value;
                return sum;
            }
        }

        /// <summary>
        ///     Returns the sum of the values in the specified collection projected by the specified selector function,
        ///     truncated to a 32-bit integer.</summary>
        public static int SumUnchecked<T>(this IEnumerable<T> source, Func<T, int> selector)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (selector == null)
                throw new ArgumentNullException(nameof(selector));
            unchecked
            {
                var sum = 0;
                foreach (var value in source)
                    sum += selector(value);
                return sum;
            }
        }

        /// <summary>
        ///     Returns only the non-<c>null</c> elements from the specified collection of nullable values as non-nullable
        ///     values.</summary>
        /// <typeparam name="T">
        ///     The inner value type.</typeparam>
        /// <param name="src">
        ///     A collection of nullable values.</param>
        /// <returns>
        ///     A collection containing only those values that aren’t <c>null</c>.</returns>
        public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> src) where T : struct
        {
            if (src == null)
                throw new ArgumentNullException(nameof(src));

            IEnumerable<T> whereNotNullIterator()
            {
                foreach (var tq in src)
                    if (tq != null)
                        yield return tq.Value;
            }
            return whereNotNullIterator();
        }

        /// <summary>
        ///     Determines whether all elements of a sequence satisfy a condition by incorporating the element's index.</summary>
        /// <typeparam name="T">
        ///     The type of the elements of <paramref name="source"/>.</typeparam>
        /// <param name="source">
        ///     An <see cref="IEnumerable{T}"/> that contains the elements to apply the predicate to.</param>
        /// <param name="predicate">
        ///     A function to test each element for a condition; the second parameter of the function represents the index of
        ///     the source element.</param>
        /// <returns>
        ///     <c>true</c> if every element of the source sequence passes the test in the specified <paramref
        ///     name="predicate"/>, or if the sequence is empty; otherwise, false.</returns>
        public static bool All<T>(this IEnumerable<T> source, Func<T, int, bool> predicate)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));
            var ix = 0;
            foreach (var elem in source)
            {
                if (!predicate(elem, ix))
                    return false;
                ix++;
            }
            return true;
        }

        /// <summary>
        ///     Determines whether any element of a sequence satisfies a condition by incorporating the element's index.</summary>
        /// <typeparam name="T">
        ///     The type of the elements of <paramref name="source"/>.</typeparam>
        /// <param name="source">
        ///     An <see cref="IEnumerable{T}"/> that contains the elements to apply the predicate to.</param>
        /// <param name="predicate">
        ///     A function to test each element for a condition; the second parameter of the function represents the index of
        ///     the source element.</param>
        /// <returns>
        ///     <c>true</c> if any elements in the source sequence pass the test in the specified <paramref
        ///     name="predicate"/>; otherwise, false.</returns>
        public static bool Any<T>(this IEnumerable<T> source, Func<T, int, bool> predicate)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));
            var ix = 0;
            foreach (var elem in source)
            {
                if (predicate(elem, ix))
                    return true;
                ix++;
            }
            return false;
        }

        /// <summary>
        ///     Converts an <c>IEnumerable&lt;KeyValuePair&lt;TKey, TValue&gt;&gt;</c> into a <c>Dictionary&lt;TKey,
        ///     TValue&gt;</c>.</summary>
        /// <param name="source">
        ///     Source collection to convert to a dictionary.</param>
        /// <param name="comparer">
        ///     An optional equality comparer to compare keys.</param>
        /// <param name="ignoreDuplicateKeys">
        ///     If <c>true</c>, duplicate keys are ignored and only their first occurrence added to the dictionary. Otherwise,
        ///     a duplicate key causes an exception.</param>
        public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> source, IEqualityComparer<TKey> comparer = null, bool ignoreDuplicateKeys = false)
        {
            if (!ignoreDuplicateKeys)
                return source.ToDictionary(kvp => kvp.Key, kvp => kvp.Value, comparer ?? EqualityComparer<TKey>.Default);
            var result = new Dictionary<TKey, TValue>();
            foreach (var entry in source)
                if (!result.ContainsKey(entry.Key))
                    result.Add(entry.Key, entry.Value);
            return result;
        }
    }

    /// <summary>
    ///     Encapsulates information about a group generated by <see
    ///     cref="IEnumerableExtensions.GroupConsecutive{TItem}(IEnumerable{TItem}, IEqualityComparer{TItem})"/> and its
    ///     overloads.</summary>
    /// <typeparam name="TItem">
    ///     Type of the elements in the sequence.</typeparam>
    /// <typeparam name="TKey">
    ///     Type of the key by which elements were compared.</typeparam>
#if EXPORT_UTIL
    public
#endif
    class ConsecutiveGroup<TItem, TKey> : IEnumerable<TItem>
    {
        /// <summary>Index in the original sequence where the group started.</summary>
        public int Index { get; private set; }
        /// <summary>Size of the group.</summary>
        public int Count { get; private set; }
        /// <summary>The key by which the items in this group are deemed equal.</summary>
        public TKey Key { get; private set; }

        private readonly IEnumerable<TItem> _group;
        internal ConsecutiveGroup(int index, List<TItem> group, TKey key)
        {
            Index = index;
            Count = group.Count;
            Key = key;
            _group = group;
        }

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.</summary>
        /// <returns>
        ///     An <see cref="IEnumerator{T}"/> that can be used to iterate through the collection.</returns>
        public IEnumerator<TItem> GetEnumerator() { return _group.GetEnumerator(); }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return GetEnumerator(); }

        /// <summary>Returns a string that represents this group’s key and its count.</summary>
        public override string ToString()
        {
            return $"{Key}; Count = {Count}";
        }
    }
}
