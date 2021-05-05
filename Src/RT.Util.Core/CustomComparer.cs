using System;
using System.Collections.Generic;
using System.Linq;

namespace RT.Util.ExtensionMethods
{
    /// <summary>Extension methods related to custom comparer.</summary>
#if EXPORT_UTIL
    public
#endif
    static class CustomComparerExtensions
    {
        /// <summary>Sorts the elements of a sequence in ascending order by using a specified comparison delegate.</summary>
        public static IEnumerable<T> OrderBy<T>(this IEnumerable<T> source, Comparison<T> comparison)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (comparison == null)
                throw new ArgumentNullException(nameof(comparison));
            return source.OrderBy(x => x, new CustomComparer<T>(comparison));
        }
    }
}

namespace RT.Util
{
    /// <summary>
    ///     Encapsulates an IComparer&lt;T&gt; that uses a comparison function provided as a delegate.</summary>
    /// <typeparam name="T">
    ///     The type of elements to be compared.</typeparam>
#if EXPORT_UTIL
    public
#endif
    sealed class CustomComparer<T> : IComparer<T>
    {
        private Comparison<T> _comparison;
        /// <summary>
        ///     Compares two elements.</summary>
        /// <remarks>
        ///     This method implements <see cref="IComparer&lt;T&gt;.Compare(T,T)"/>.</remarks>
        public int Compare(T x, T y) { return _comparison(x, y); }

        /// <summary>
        ///     Constructor.</summary>
        /// <param name="comparison">
        ///     Provides the comparison function for this comparer.</param>
        public CustomComparer(Comparison<T> comparison) { _comparison = comparison; }

        /// <summary>
        ///     Creates and returns a CustomComparer which compares items by comparing the results of a selector function.</summary>
        /// <param name="selector">
        ///     Function selecting the actual value to be compared.</param>
        /// <param name="comparer">
        ///     Comparer to use for comparing the results of the selector function. If null, the default comparer is used;
        ///     this comparer will use the IComparable interface if implemented.</param>
        public static CustomComparer<T> By<TBy>(Func<T, TBy> selector, IComparer<TBy> comparer = null)
        {
            if (selector == null) throw new ArgumentNullException(nameof(selector));
            if (comparer == null)
                comparer = Comparer<TBy>.Default;
            return new CustomComparer<T>((a, b) => comparer.Compare(selector(a), selector(b)));
        }

        /// <summary>
        ///     Creates and returns a CustomComparer which compares items by comparing the results of a selector function.</summary>
        /// <param name="selector">
        ///     Function selecting the actual value to be compared.</param>
        /// <param name="comparison">
        ///     Comparison to use for comparing the results of the selector function.</param>
        public static CustomComparer<T> By<TBy>(Func<T, TBy> selector, Comparison<TBy> comparison)
        {
            if (selector == null) throw new ArgumentNullException(nameof(selector));
            if (comparison == null) throw new ArgumentNullException(nameof(comparison));
            return new CustomComparer<T>((a, b) => comparison(selector(a), selector(b)));
        }

        /// <summary>
        ///     Creates and returns a CustomComparer which compares items by comparing the results of a selector function.</summary>
        /// <param name="selector">
        ///     Function selecting the string value to be compared.</param>
        /// <param name="ignoreCase">
        ///     If false, an invariant culture string comparison is used. Otherwise, an ordinal no-case comparison (suitable
        ///     for filenames etc).</param>
        public static CustomComparer<T> By(Func<T, string> selector, bool ignoreCase)
        {
            if (selector == null) throw new ArgumentNullException(nameof(selector));
            var comparer = ignoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.InvariantCulture;
            return new CustomComparer<T>((a, b) => comparer.Compare(selector(a), selector(b)));
        }

        /// <summary>
        ///     Creates and returns a CustomComparer which uses the current comparer first, and if the current comparer says
        ///     the items are equal, further compares items by comparing the results of a selector function.</summary>
        /// <param name="selector">
        ///     Function selecting the actual value to be compared.</param>
        /// <param name="comparer">
        ///     Comparer to use for comparing the results of the selector function. If null, the default comparer is used;
        ///     this comparer will use the IComparable interface if implemented.</param>
        public CustomComparer<T> ThenBy<TBy>(Func<T, TBy> selector, IComparer<TBy> comparer = null)
        {
            if (selector == null) throw new ArgumentNullException(nameof(selector));
            if (comparer == null)
                comparer = Comparer<TBy>.Default;
            return new CustomComparer<T>((a, b) =>
            {
                int result = Compare(a, b);
                if (result != 0)
                    return result;
                else
                    return comparer.Compare(selector(a), selector(b));
            });
        }

        /// <summary>
        ///     Creates and returns a CustomComparer which uses the current comparer first, and if the current comparer says
        ///     the items are equal, further compares items by comparing the results of a selector function.</summary>
        /// <param name="selector">
        ///     Function selecting the actual value to be compared.</param>
        /// <param name="comparison">
        ///     Comparison to use for comparing the results of the selector function.</param>
        public CustomComparer<T> ThenBy<TBy>(Func<T, TBy> selector, Comparison<TBy> comparison)
        {
            if (selector == null) throw new ArgumentNullException(nameof(selector));
            return new CustomComparer<T>((a, b) =>
            {
                int result = Compare(a, b);
                if (result != 0)
                    return result;
                else
                    return comparison(selector(a), selector(b));
            });
        }

        /// <summary>
        ///     Creates and returns a CustomComparer which uses the current comparer first, and if the current comparer says
        ///     the items are equal, further compares items by comparing the results of a string selector function.</summary>
        /// <param name="selector">
        ///     Function selecting the actual value to be compared.</param>
        /// <param name="ignoreCase">
        ///     If false, an invariant culture string comparison is used. Otherwise, an ordinal no-case comparison (suitable
        ///     for filenames etc).</param>
        public CustomComparer<T> ThenBy(Func<T, string> selector, bool ignoreCase)
        {
            if (selector == null) throw new ArgumentNullException(nameof(selector));
            var comparer = ignoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.InvariantCulture;
            return new CustomComparer<T>((a, b) =>
            {
                int result = Compare(a, b);
                if (result != 0)
                    return result;
                else
                    return comparer.Compare(selector(a), selector(b));
            });
        }
    }

    /// <summary>
    ///     Encapsulates an IEqualityComparer&lt;T&gt; that uses an equality comparison function provided as a delegate.</summary>
    /// <typeparam name="T">
    ///     The type of elements to be compared for equality.</typeparam>
#if EXPORT_UTIL
    public
#endif
    sealed class CustomEqualityComparer<T> : IEqualityComparer<T>
    {
        private Func<T, T, bool> _comparison;
        private Func<T, int> _getHashCode;

        /// <summary>
        ///     Compares two elements for equality.</summary>
        /// <remarks>
        ///     This method implements <see cref="IEqualityComparer&lt;T&gt;.Equals(T,T)"/>.</remarks>
        public bool Equals(T x, T y) { return _comparison(x, y); }
        /// <summary>
        ///     Returns a hash code for an element.</summary>
        /// <remarks>
        ///     This method implements <see cref="IEqualityComparer&lt;T&gt;.GetHashCode(T)"/>.</remarks>
        public int GetHashCode(T obj) { return _getHashCode == null ? obj.GetHashCode() : _getHashCode(obj); }

        /// <summary>
        ///     Constructor.</summary>
        /// <param name="comparison">
        ///     Provides the comparison function for this equality comparer.</param>
        /// <param name="getHashCode">
        ///     Provides the hash function for this equality comparer.</param>
        public CustomEqualityComparer(Func<T, T, bool> comparison, Func<T, int> getHashCode) { _comparison = comparison; _getHashCode = getHashCode; }

        /// <summary>
        ///     Constructor which re-uses the default hash function. Use this overload only if using the objects’ original
        ///     hash function is appropriate for this equality comparison.</summary>
        /// <param name="comparison">
        ///     Provides the comparison function for this equality comparer.</param>
        public CustomEqualityComparer(Func<T, T, bool> comparison) { _comparison = comparison; _getHashCode = null; }

        /// <summary>
        ///     Creates and returns an equality comparer that compares the equality of objects by comparing the equality of
        ///     the result of a selector function.</summary>
        /// <param name="selector">
        ///     Function selecting the actual value to be compared.</param>
        /// <param name="comparer">
        ///     Comparer to use for comparing the results of the selector function.</param>
        public static CustomEqualityComparer<T> By<TBy>(Func<T, TBy> selector, IEqualityComparer<TBy> comparer)
        {
            if (selector == null) throw new ArgumentNullException(nameof(selector));
            if (comparer == null) throw new ArgumentNullException(nameof(comparer));
            return new CustomEqualityComparer<T>((a, b) => comparer.Equals(selector(a), selector(b)), a => comparer.GetHashCode(selector(a)));
        }

        /// <summary>
        ///     Creates and returns an equality comparer that compares the equality of objects by comparing the equality of
        ///     the result of a selector function.</summary>
        /// <param name="selector">
        ///     Function selecting the actual value to be compared.</param>
        /// <param name="comparison">
        ///     Function used to compare values for equality. If null, will use IEquatable if implemented, or the object's
        ///     Equals override.</param>
        /// <param name="getHashCode">
        ///     Function used to compute hash codes. If null, will use IEquatable if implemented, or the object's GetHashCode
        ///     override.</param>
        public static CustomEqualityComparer<T> By<TBy>(Func<T, TBy> selector, Func<TBy, TBy, bool> comparison = null, Func<TBy, int> getHashCode = null)
        {
            if (selector == null) throw new ArgumentNullException(nameof(selector));
            var default_ = EqualityComparer<TBy>.Default;
            var cmp = comparison == null
                ? new Func<T, T, bool>((T a, T b) => default_.Equals(selector(a), selector(b)))
                : new Func<T, T, bool>((T a, T b) => comparison(selector(a), selector(b)));
            var ghc = getHashCode == null
                ? new Func<T, int>((T a) => default_.GetHashCode(selector(a)))
                : new Func<T, int>((T a) => getHashCode(selector(a)));
            return new CustomEqualityComparer<T>(cmp, ghc);
        }

        /// <summary>
        ///     Creates and returns an equality comparer that compares the equality of objects by comparing the equality of
        ///     the result of a string selector function.</summary>
        /// <param name="selector">
        ///     Function selecting the actual string value to be compared.</param>
        /// <param name="ignoreCase">
        ///     If false, an invariant culture string comparison is used. Otherwise, an ordinal no-case comparison (suitable
        ///     for filenames etc).</param>
        public static CustomEqualityComparer<T> By(Func<T, string> selector, bool ignoreCase)
        {
            if (selector == null) throw new ArgumentNullException(nameof(selector));
            return CustomEqualityComparer<T>.By(selector, ignoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.InvariantCulture);
        }
    }
}
