using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace RT.Util.Collections
{
    /// <summary>
    ///     Encapsulates a list which dynamically grows as items are written to non-existent indexes. Any gaps are populated
    ///     with default values. The behaviour of this list's indexed getter and setter is indistinguishable from that of an
    ///     infinitely long list pre-populated by invoking the initializer function (assuming it is side-effect free). See
    ///     Remarks.</summary>
    /// <remarks>
    ///     <para>
    ///         Only the indexer behaviour is changed; in every other way this behaves just like a standard, non-infinite
    ///         list. Moreover, the implementation is such that the new behaviour is only effective when used directly through
    ///         the class; accessing the indexer through the <c>IList</c> interface or the <c>List</c> base class will
    ///         currently behave the same as it would for a standard list.</para>
    ///     <para>
    ///         Note that this is not a sparse list; accessing elements at a given index will grow the list to contain all of
    ///         the items below the index too.</para></remarks>
    public class AutoList<T> : IList<T>
    {
        private readonly List<T> _inner;
        private readonly Func<int, T> _initializer;

        /// <summary>
        ///     Gets or sets the element at the specified index. The behaviour of both the getter and the setter is
        ///     indistinguishable from that of an infinitely long list pre-populated by invoking the initializer function
        ///     (assuming it is side-effect free).</summary>
        public T this[int index]
        {
            get
            {
                // default(T) cannot possibly create a value that we'd need to store immediately in order to preserve the infinite list illusion,
                // so do not grow the list for this if the user supplied no initializer.
                if (_initializer == null)
                    return index >= Count ? default(T) : _inner[index];

                fill(index + 1);
                return _inner[index];
            }

            set
            {
                fill(index + 1);
                _inner[index] = value;
            }
        }

        private void fill(int index)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index), "'index' cannot be negative.");
            while (index > Count)
                Add(_initializer == null ? default(T) : _initializer(Count));
        }
        private void fill(int index, int count)
        {
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), "'count' cannot be negative.");
            fill(index + count);
        }

        /// <summary>
        ///     Constructor.</summary>
        /// <param name="initializer">
        ///     A function which creates a value to be used for non-existent elements upon their creation. If <c>null</c>,
        ///     <c>default(T)</c> is used instead.</param>
        public AutoList(Func<int, T> initializer = null)
        {
            _inner = new List<T>();
            _initializer = initializer;
        }

        /// <summary>
        ///     Constructor.</summary>
        /// <param name="capacity">
        ///     The number of elements that the new list can initially store.</param>
        /// <param name="initializer">
        ///     A function which creates a value to be used for non-existent elements upon their creation. If <c>null</c>,
        ///     <c>default(T)</c> is used instead.</param>
        public AutoList(int capacity, Func<int, T> initializer = null)
        {
            _inner = new List<T>(capacity);
            _initializer = initializer;
        }

        /// <summary>
        ///     Constructor.</summary>
        /// <param name="collection">
        ///     A collection whose elements are copied to the new list.</param>
        /// <param name="initializer">
        ///     A function which creates a value to be used for non-existent elements upon their creation. If <c>null</c>,
        ///     <c>default(T)</c> is used instead.</param>
        public AutoList(IEnumerable<T> collection, Func<int, T> initializer = null)
        {
            _inner = new List<T>(collection);
            _initializer = initializer;
        }

        /// <summary>Equivalent to the same method in <see cref="List{T}"/>.</summary>
        public int IndexOf(T item) { return _inner.IndexOf(item); }
        /// <summary>Equivalent to the same method in <see cref="List{T}"/>.</summary>
        public void Insert(int index, T item) { fill(index); _inner.Insert(index, item); }
        /// <summary>Equivalent to the same method in <see cref="List{T}"/>.</summary>
        public void RemoveAt(int index) { fill(index); _inner.RemoveAt(index); }
        /// <summary>Equivalent to the same method in <see cref="List{T}"/>.</summary>
        public void Add(T item) { _inner.Add(item); }
        /// <summary>Equivalent to the same method in <see cref="List{T}"/>.</summary>
        public void Clear() { _inner.Clear(); }
        /// <summary>Equivalent to the same method in <see cref="List{T}"/>.</summary>
        public bool Contains(T item) { return _inner.Contains(item); }
        /// <summary>Equivalent to the same method in <see cref="List{T}"/>.</summary>
        public void CopyTo(T[] array, int arrayIndex) { _inner.CopyTo(array, arrayIndex); }
        /// <summary>Equivalent to the same property in <see cref="List{T}"/>.</summary>
        public int Count { get { return _inner.Count; } }
        /// <summary>Equivalent to the same property in <see cref="List{T}"/>.</summary>
        public bool IsReadOnly { get { return false; } }
        /// <summary>Equivalent to the same method in <see cref="List{T}"/>.</summary>
        public bool Remove(T item) { return _inner.Remove(item); }
        /// <summary>Equivalent to the same method in <see cref="List{T}"/>.</summary>
        public IEnumerator<T> GetEnumerator() { return _inner.GetEnumerator(); }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return _inner.GetEnumerator(); }

        /// <summary>Equivalent to the same method in <see cref="List{T}"/>.</summary>
        public void AddRange(IEnumerable<T> item) { _inner.AddRange(item); }
        /// <summary>Equivalent to the same method in <see cref="List{T}"/>.</summary>
        public ReadOnlyCollection<T> AsReadOnly() { return new ReadOnlyCollection<T>(this); }
        /// <summary>Equivalent to the same method in <see cref="List{T}"/>.</summary>
        public int BinarySearch(T item) { return _inner.BinarySearch(item); }
        /// <summary>Equivalent to the same property in <see cref="List{T}"/>.</summary>
        public int Capacity
        {
            get { return _inner.Capacity; }
            set { _inner.Capacity = value; }
        }
        /// <summary>Equivalent to the same method in <see cref="List{T}"/>.</summary>
        public AutoList<TOutput> ConvertAll<TOutput>(Converter<T, TOutput> converter)
        {
            if (converter == null)
                throw new ArgumentNullException(nameof(converter));
            var list = new AutoList<TOutput>(_inner.Count);
            foreach (var item in _inner)
                list.Add(converter(item));
            return list;
        }
        /// <summary>Equivalent to the same method in <see cref="List{T}"/>.</summary>
        public bool Exists(Predicate<T> match) { return _inner.Exists(match); }
        /// <summary>Equivalent to the same method in <see cref="List{T}"/>.</summary>
        public T Find(Predicate<T> match) { return _inner.Find(match); }
        /// <summary>Equivalent to the same method in <see cref="List{T}"/>.</summary>
        public AutoList<T> FindAll(Predicate<T> match)
        {
            if (match == null)
                throw new ArgumentNullException(nameof(match));
            var list = new AutoList<T>();
            foreach (var item in _inner)
                if (match(item))
                    list.Add(item);
            return list;
        }
        /// <summary>Equivalent to the same method in <see cref="List{T}"/>.</summary>
        public int FindIndex(Predicate<T> match) { return _inner.FindIndex(match); }
        /// <summary>Equivalent to the same method in <see cref="List{T}"/>.</summary>
        public int FindIndex(int startIndex, Predicate<T> match) { fill(startIndex); return _inner.FindIndex(startIndex, match); }
        /// <summary>Equivalent to the same method in <see cref="List{T}"/>.</summary>
        public int FindIndex(int startIndex, int count, Predicate<T> match) { fill(startIndex, count); return _inner.FindIndex(startIndex, count, match); }
        /// <summary>Equivalent to the same method in <see cref="List{T}"/>.</summary>
        public T FindLast(Predicate<T> match) { return _inner.FindLast(match); }
        /// <summary>Equivalent to the same method in <see cref="List{T}"/>.</summary>
        public int FindLastIndex(Predicate<T> match) { return _inner.FindLastIndex(match); }
        /// <summary>Equivalent to the same method in <see cref="List{T}"/>.</summary>
        public int FindLastIndex(int startIndex, Predicate<T> match) { fill(startIndex); return _inner.FindLastIndex(startIndex, match); }
        /// <summary>Equivalent to the same method in <see cref="List{T}"/>.</summary>
        public int FindLastIndex(int startIndex, int count, Predicate<T> match) { fill(startIndex, count); return _inner.FindLastIndex(startIndex, count, match); }
        /// <summary>Equivalent to the same method in <see cref="List{T}"/>.</summary>
        public AutoList<T> GetRange(int index, int count)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index), "'index' cannot be negative.");
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), "'count' cannot be negative.");
            fill(index, count);
            while (_inner.Count < index + count)
                _inner.Add(_initializer == null ? default(T) : _initializer(_inner.Count));
            var list = new AutoList<T>(count);
            for (int i = 0; i < count; i++)
                list.Add(_inner[i + index]);
            return list;
        }
        /// <summary>Equivalent to the same method in <see cref="List{T}"/>.</summary>
        public int IndexOf(T item, int index) { fill(index); return _inner.IndexOf(item, index); }
        /// <summary>Equivalent to the same method in <see cref="List{T}"/>.</summary>
        public int IndexOf(T item, int index, int count) { fill(index, count); return _inner.IndexOf(item, index, count); }

        /// <summary>Equivalent to the same method in <see cref="List{T}"/>.</summary>
        public void InsertRange(int index, IEnumerable<T> collection) { fill(index); _inner.InsertRange(index, collection); }
        /// <summary>Equivalent to the same method in <see cref="List{T}"/>.</summary>
        public int LastIndexOf(T item) { return _inner.LastIndexOf(item); }
        /// <summary>Equivalent to the same method in <see cref="List{T}"/>.</summary>
        public int LastIndexOf(T item, int index) { fill(index); return _inner.LastIndexOf(item, index); }
        /// <summary>Equivalent to the same method in <see cref="List{T}"/>.</summary>
        public int LastIndexOf(T item, int index, int count) { fill(index, count); return _inner.LastIndexOf(item, index, count); }
        /// <summary>Equivalent to the same method in <see cref="List{T}"/>.</summary>
        public int RemoveAll(Predicate<T> match) { return _inner.RemoveAll(match); }
        /// <summary>Equivalent to the same method in <see cref="List{T}"/>.</summary>
        public void RemoveRange(int index, int count) { fill(index, count); _inner.RemoveRange(index, count); }
        /// <summary>Equivalent to the same method in <see cref="List{T}"/>.</summary>
        public void Reverse() { _inner.Reverse(); }
        /// <summary>Equivalent to the same method in <see cref="List{T}"/>.</summary>
        public void Reverse(int index, int count) { fill(index, count); _inner.Reverse(index, count); }
        /// <summary>Equivalent to the same method in <see cref="List{T}"/>.</summary>
        public void Sort() { _inner.Sort(); }
        /// <summary>Equivalent to the same method in <see cref="List{T}"/>.</summary>
        public void Sort(Comparison<T> comparison) { _inner.Sort(comparison); }
        /// <summary>Equivalent to the same method in <see cref="List{T}"/>.</summary>
        public void Sort(IComparer<T> comparer) { _inner.Sort(comparer); }
        /// <summary>Equivalent to the same method in <see cref="List{T}"/>.</summary>
        public void Sort(int index, int count, IComparer<T> comparer) { fill(index, count); _inner.Sort(index, count, comparer); }
        /// <summary>Equivalent to the same method in <see cref="List{T}"/>.</summary>
        public T[] ToArray() { return _inner.ToArray(); }
        /// <summary>Equivalent to the same method in <see cref="List{T}"/>.</summary>
        public void TrimExcess() { _inner.TrimExcess(); }
        /// <summary>Equivalent to the same method in <see cref="List{T}"/>.</summary>
        public bool TrueForAll(Predicate<T> match) { return _inner.TrueForAll(match); }
    }
}
