using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace RT.Util.ExtensionMethods
{
    using RT.Util.Collections;

    /// <summary>Extension methods related to read-only collections.</summary>
    public static class ReadOnlyCollectionExtensions
    {
        /// <summary>
        ///     Creates and returns a read-only wrapper around this collection. Note: a new wrapper is created on every call.
        ///     Consider caching it.</summary>
        public static ReadOnlyCollection<T> AsReadOnly<T>(this ICollection<T> coll)
        {
            if (coll == null)
                throw new ArgumentNullException(nameof(coll));
            return new ReadOnlyCollection<T>(coll);
        }

        /// <summary>
        ///     Gets a read-only wrapper around this collection. If <paramref name="cache"/> is already a wrapper for this
        ///     collection returns that, otherwise creates a new wrapper, stores it in <paramref name="cache"/>, and returns
        ///     that.</summary>
        public static ReadOnlyCollection<T> AsReadOnly<T>(this ICollection<T> coll, ref ReadOnlyCollection<T> cache)
        {
            if (coll == null)
                throw new ArgumentNullException(nameof(coll));
            if (cache == null || !cache.IsWrapperFor(coll))
                cache = new ReadOnlyCollection<T>(coll);
            return cache;
        }

        /// <summary>
        ///     Creates and returns a read-only wrapper around this dictionary. Note: a new wrapper is created on every call.
        ///     Consider caching it.</summary>
        public static ReadOnlyDictionary<TK, TV> AsReadOnly<TK, TV>(this IDictionary<TK, TV> dict)
        {
            if (dict == null)
                throw new ArgumentNullException(nameof(dict));
            return new ReadOnlyDictionary<TK, TV>(dict);
        }

        /// <summary>
        ///     Gets a read-only wrapper around this dictionary. If <paramref name="cache"/> is already a wrapper for this
        ///     dictionary returns that, otherwise creates a new wrapper, stores it in <paramref name="cache"/>, and returns
        ///     that.</summary>
        public static ReadOnlyDictionary<TK, TV> AsReadOnly<TK, TV>(this IDictionary<TK, TV> dict, ref ReadOnlyDictionary<TK, TV> cache)
        {
            if (dict == null)
                throw new ArgumentNullException(nameof(dict));
            if (cache == null || !cache.IsWrapperFor(dict))
                cache = new ReadOnlyDictionary<TK, TV>(dict);
            return cache;
        }
    }
}

namespace RT.Util.Collections
{
    /// <summary>
    ///     Wraps an <see cref="IDictionary&lt;TKey,TValue&gt;"/> to allow reading values but prevent setting/removing them.</summary>
    [DebuggerDisplay("Count = {Count}")]
    [DebuggerTypeProxy(typeof(ReadOnlyDictionary<,>.DebugView))]
    public sealed class ReadOnlyDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        private IDictionary<TKey, TValue> _dict;
        private ReadOnlyCollection<TKey> _keys;
        private ReadOnlyCollection<TValue> _values;

        /// <summary>Creates a new read-only wrapper for the specified <see cref="IDictionary&lt;TKey,TValue&gt;"/>.</summary>
        public ReadOnlyDictionary(IDictionary<TKey, TValue> dict)
        {
            _dict = dict;
        }

        /// <summary>Returns true if <paramref name="dict"/> is the same dictionary object as the one this class wraps.</summary>
        public bool IsWrapperFor(IDictionary<TKey, TValue> dict)
        {
            return object.ReferenceEquals(dict, _dict);
        }

        /// <summary>Not supported on a ReadOnlyDictionary.</summary>
        public void Add(TKey key, TValue value)
        {
            throw new InvalidOperationException("Cannot Add to a read-only dictionary");
        }

        /// <summary>Returns true if the dictionary contains the specified key.</summary>
        public bool ContainsKey(TKey key)
        {
            return _dict.ContainsKey(key);
        }

        /// <summary>Gets a read-only collection of keys in this dictionary.</summary>
        public ICollection<TKey> Keys
        {
            get
            {
                if (_keys == null)
                    _keys = new ReadOnlyCollection<TKey>(_dict.Keys);
                return _keys;
            }
        }

        /// <summary>Not supported on a ReadOnlyDictionary.</summary>
        public bool Remove(TKey key)
        {
            throw new InvalidOperationException("Cannot Remove to a read-only dictionary");
        }

        /// <summary>Gets the value associated with the specified key. Returns true if the value exists, false otherwise.</summary>
        public bool TryGetValue(TKey key, out TValue value)
        {
            return _dict.TryGetValue(key, out value);
        }

        /// <summary>Gets a read-only collection of values in this dictionary.</summary>
        public ICollection<TValue> Values
        {
            get
            {
                if (_values == null)
                    _values = new ReadOnlyCollection<TValue>(_dict.Values);
                return _values;
            }
        }

        /// <summary>Gets a value from the dictionary. Setting values is not supported on a ReadOnlyDictionary.</summary>
        public TValue this[TKey key]
        {
            get
            {
                return _dict[key];
            }
            set
            {
                throw new InvalidOperationException("Cannot set a value in a read-only dictionary");
            }
        }

        /// <summary>Not supported on a ReadOnlyDictionary.</summary>
        public void Add(KeyValuePair<TKey, TValue> item)
        {
            throw new InvalidOperationException("Cannot Add to a read-only dictionary");
        }

        /// <summary>Not supported on a ReadOnlyDictionary.</summary>
        public void Clear()
        {
            throw new InvalidOperationException("Cannot Clear to a read-only dictionary");
        }

        /// <summary>Returns true if the dictionary contains the specified key/value pair.</summary>
        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return _dict.Contains(item);
        }

        /// <summary>Copies the key/value pairs of this dictionary into the specified array.</summary>
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            _dict.CopyTo(array, arrayIndex);
        }

        /// <summary>Gets the number of elements in this read-only collection.</summary>
        public int Count
        {
            get { return _dict.Count; }
        }

        /// <summary>Returns true, as this is a read-only collection.</summary>
        public bool IsReadOnly
        {
            get { return true; }
        }

        /// <summary>Not supported on a ReadOnlyDictionary.</summary>
        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            throw new InvalidOperationException("Cannot Remove from a read-only dictionary");
        }

        /// <summary>Gets an enumerator for the key/value pairs stored in this dictionary.</summary>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _dict.GetEnumerator();
        }

        /// <summary>Gets an enumerator for the key/value pairs stored in this dictionary.</summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _dict.GetEnumerator();
        }

        internal sealed class DebugView
        {
            private ReadOnlyDictionary<TKey, TValue> _dict;

            public DebugView(ReadOnlyDictionary<TKey, TValue> dict)
            {
                _dict = dict;
            }

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public KeyValuePair<TKey, TValue>[] Items { get { return _dict.ToArray(); } }
        }
    }

    /// <summary>Wraps an <see cref="ICollection&lt;T&gt;"/> to allow reading values but prevent setting/removing them.</summary>
    [DebuggerDisplay("Count = {Count}")]
    [DebuggerTypeProxy(typeof(ReadOnlyCollection<>.DebugView))]
    public sealed class ReadOnlyCollection<T> : ICollection<T>
    {
        private ICollection<T> _coll;

        /// <summary>Creates a new read-only wrapper for the specified <see cref="ICollection&lt;T&gt;"/>.</summary>
        public ReadOnlyCollection(ICollection<T> coll)
        {
            _coll = coll;
        }

        /// <summary>Returns true if <paramref name="coll"/> is the same collection object as the one this class wraps.</summary>
        public bool IsWrapperFor(ICollection<T> coll)
        {
            return object.ReferenceEquals(coll, _coll);
        }

        /// <summary>Not supported on a ReadOnlyCollection.</summary>
        public void Add(T item)
        {
            throw new InvalidOperationException("Cannot Add to a read-only collection");
        }

        /// <summary>Not supported on a ReadOnlyCollection.</summary>
        public void Clear()
        {
            throw new InvalidOperationException("Cannot Clear a read-only collection");
        }

        /// <summary>Returns true if the specified item exists in this collection, false otherwise.</summary>
        public bool Contains(T item)
        {
            return _coll.Contains(item);
        }

        /// <summary>Copies the values stored in this collection into the specified dictionary.</summary>
        public void CopyTo(T[] array, int arrayIndex)
        {
            _coll.CopyTo(array, arrayIndex);
        }

        /// <summary>Gets the number of values stored in this collection.</summary>
        public int Count
        {
            get { return _coll.Count; }
        }

        /// <summary>Returns true, as this is a read-only collection.</summary>
        public bool IsReadOnly
        {
            get { return true; }
        }

        /// <summary>Not supported on a ReadOnlyCollection.</summary>
        public bool Remove(T item)
        {
            throw new InvalidOperationException("Cannot Remove from a read-only collection");
        }

        /// <summary>Gets an enumerator for the values stored in this collection.</summary>
        public IEnumerator<T> GetEnumerator()
        {
            return _coll.GetEnumerator();
        }

        /// <summary>Gets an enumerator for the values stored in this collection.</summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _coll.GetEnumerator();
        }

        internal sealed class DebugView
        {
            private ReadOnlyCollection<T> _collection;

            public DebugView(ReadOnlyCollection<T> collection)
            {
                _collection = collection;
            }

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public T[] Items { get { return _collection.ToArray(); } }
        }
    }
}
