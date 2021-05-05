using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RT.Util.Collections
{
    /// <summary>
    ///     Implements a list whose items are always stored in a sorted order. Multiple equal items are allowed, and will
    ///     always be added to the end of a run of equal items. Insertion, and removal are O(N); lookups are O(log N); access
    ///     by index is supported.</summary>
    [Serializable]
    public sealed class ListSorted<T> : IList<T>
    {
        private List<T> _list;
        private IComparer<T> _comparer;

        /// <summary>Creates an empty <see cref="ListSorted&lt;T&gt;"/> using a default comparer for the item type.</summary>
        public ListSorted()
        {
            _list = new List<T>();
            _comparer = Comparer<T>.Default;
        }

        /// <summary>
        ///     Creates an empty <see cref="ListSorted&lt;T&gt;"/> of the specified capacity and using a default comparer for
        ///     the item type.</summary>
        public ListSorted(int capacity)
        {
            _list = new List<T>(capacity);
            _comparer = Comparer<T>.Default;
        }

        /// <summary>Creates an empty <see cref="ListSorted&lt;T&gt;"/> using the specified item comparer.</summary>
        public ListSorted(IComparer<T> comparer)
        {
            _list = new List<T>();
            _comparer = comparer;
        }

        /// <summary>
        ///     Creates an empty <see cref="ListSorted&lt;T&gt;"/> of the specified capacity and using the specified item
        ///     comparer.</summary>
        public ListSorted(int capacity, IComparer<T> comparer)
        {
            _list = new List<T>(capacity);
            _comparer = comparer;
        }

        /// <summary>
        ///     Adds all of the specified items into the list. The resulting list will contain all the items in the same order
        ///     as a stable sort would have produced. NOTE: the current implementation is O(N log N) if the collection is
        ///     empty, or O(N*M) otherwise, where N = items currently in the collection and M = items to be added. The latter
        ///     can be improved.</summary>
        public void AddRange(IEnumerable<T> collection)
        {
            if (_list.Count == 0)
                _list = new List<T>(collection.OrderBy(x => x, _comparer));
            else
                foreach (var item in collection)
                    Add(item);
        }

        /// <summary>
        ///     Returns the index of the FIRST item equal to the specified item, or -1 if the item is not found. The operation
        ///     is O(log N).</summary>
        public int IndexOf(T item)
        {
            int i = _list.BinarySearch(item, _comparer);
            if (i < 0)
                return -1;
            while (i > 0 && _comparer.Compare(_list[i - 1], item) == 0)
                i--;
            return i;
        }

        /// <summary>
        ///     Returns the index of the LAST item equal to the specified item, or -1 if the item is not found. The operation
        ///     is O(log N).</summary>
        public int LastIndexOf(T item)
        {
            int i = _list.BinarySearch(item, _comparer);
            if (i < 0)
                return -1;
            while (i < _list.Count - 1 && _comparer.Compare(_list[i + 1], item) == 0)
                i++;
            return i;
        }

        /// <summary>Not supported in this class, will always throw an exception.</summary>
        void IList<T>.Insert(int index, T item)
        {
            throw new InvalidOperationException("Cannot insert an item at an arbitrary index into a ListSorted.");
        }

        /// <summary>Removes the item at the specified index.</summary>
        public void RemoveAt(int index)
        {
            _list.RemoveAt(index);
        }

        /// <summary>Gets an item at the specified index. Setting an item is not supported and will always throw an exception.</summary>
        public T this[int index]
        {
            get
            {
                return _list[index];
            }
            set
            {
                throw new InvalidOperationException("Cannot set an item at an arbitrary index in a ListSorted.");
            }
        }

        /// <summary>
        ///     Adds the specified item to the list. The item is added at the appropriate location to keep the list sorted. If
        ///     multiple equal items are stored, this method is guaranteed to add an item at the end of the equal items run.
        ///     This method is O(N).</summary>
        public void Add(T item)
        {
            int i = _list.BinarySearch(item, _comparer);
            if (i < 0)
                i = ~i;
            else
                do i++; while (i < _list.Count && _comparer.Compare(_list[i], item) == 0);

            _list.Insert(i, item);
        }

        /// <summary>Removes all items from the list.</summary>
        public void Clear()
        {
            _list.Clear();
        }

        /// <summary>Returns true if the list contains the specified item. This operation is O(logN).</summary>
        public bool Contains(T item)
        {
            return _list.BinarySearch(item, _comparer) >= 0;
        }

        /// <summary>
        ///     Copies all items of this collection into the specified array, starting at the specified index, in the sorted
        ///     order.</summary>
        public void CopyTo(T[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }

        /// <summary>Gets the number of items stored in this collection.</summary>
        public int Count
        {
            get { return _list.Count; }
        }

        /// <summary>Returns false.</summary>
        public bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        ///     Removes the FIRST occurrence of the specified item from the list. Returns true if the item was removed, or
        ///     false if it wasn't found.</summary>
        public bool Remove(T item)
        {
            int i = IndexOf(item);
            if (i < 0) return false;
            _list.RemoveAt(i);
            return true;
        }

        /// <summary>
        ///     Removes the LAST occurrence of the specified item from the list. Returns true if the item was removed, or
        ///     false if it wasn't found.</summary>
        public bool RemoveLast(T item)
        {
            int i = LastIndexOf(item);
            if (i < 0) return false;
            _list.RemoveAt(i);
            return true;
        }

        /// <summary>Gets an enumerator which enumerates all items of this collection in sorted order.</summary>
        public IEnumerator<T> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        /// <summary>Gets an enumerator which enumerates all items of this collection in sorted order.</summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _list.GetEnumerator();
        }
    }
}
