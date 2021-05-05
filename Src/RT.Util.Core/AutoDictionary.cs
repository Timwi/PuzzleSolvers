using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace RT.Util.Collections
{
    /// <summary>
    ///     Implements a dictionary with an indexer which automatically inserts missing keys on reads. In all other ways it
    ///     behaves exactly the same as a standard dictionary.</summary>
    public class AutoDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        private Dictionary<TKey, TValue> _inner;
        private Func<TKey, TValue> _initializer;

        /// <summary>
        ///     Gets or sets the element with the specified key. When getting a key that hasn't been set before, the key is
        ///     added to the dictionary with a value returned by the initializer as configured in the constructor.</summary>
        public TValue this[TKey key]
        {
            get
            {
                if (_inner.TryGetValue(key, out TValue v))
                    return v;
                v = _initializer == null ? default(TValue) : _initializer(key);
                _inner.Add(key, v);
                return v;
            }

            set
            {
                _inner[key] = value;
            }
        }

        /// <summary>Constructor.</summary>
        public AutoDictionary(Func<TKey, TValue> initializer = null)
        {
            _inner = new Dictionary<TKey, TValue>();
            _initializer = initializer;
        }
        /// <summary>Constructor.</summary>
        public AutoDictionary(int capacity, Func<TKey, TValue> initializer = null)
        {
            _inner = new Dictionary<TKey, TValue>(capacity);
            _initializer = initializer;
        }
        /// <summary>Constructor.</summary>
        public AutoDictionary(IEqualityComparer<TKey> comparer, Func<TKey, TValue> initializer = null)
        {
            _inner = new Dictionary<TKey, TValue>(comparer);
            _initializer = initializer;
        }
        /// <summary>Constructor.</summary>
        public AutoDictionary(IDictionary<TKey, TValue> dictionary, Func<TKey, TValue> initializer = null)
        {
            _inner = new Dictionary<TKey, TValue>(dictionary);
            _initializer = initializer;
        }
        /// <summary>Constructor.</summary>
        public AutoDictionary(int capacity, IEqualityComparer<TKey> comparer, Func<TKey, TValue> initializer = null)
        {
            _inner = new Dictionary<TKey, TValue>(capacity, comparer);
            _initializer = initializer;
        }
        /// <summary>Constructor.</summary>
        public AutoDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer, Func<TKey, TValue> initializer = null)
        {
            _inner = new Dictionary<TKey, TValue>(dictionary, comparer);
            _initializer = initializer;
        }

        /// <summary>Equivalent to the same property in <see cref="Dictionary{TKey,TValue}"/>.</summary>
        public IEqualityComparer<TKey> Comparer { get { return _inner.Comparer; } }
        /// <summary>Equivalent to the same property in <see cref="Dictionary{TKey,TValue}"/>.</summary>
        public int Count { get { return _inner.Count; } }
        /// <summary>Equivalent to the same property in <see cref="Dictionary{TKey,TValue}"/>.</summary>
        public Dictionary<TKey, TValue>.KeyCollection Keys { get { return _inner.Keys; } }
        /// <summary>Equivalent to the same property in <see cref="Dictionary{TKey,TValue}"/>.</summary>
        public Dictionary<TKey, TValue>.ValueCollection Values { get { return _inner.Values; } }

        /// <summary>Equivalent to the same method in <see cref="Dictionary{TKey,TValue}"/>.</summary>
        public void Add(TKey key, TValue value) { _inner.Add(key, value); }
        /// <summary>Equivalent to the same method in <see cref="Dictionary{TKey,TValue}"/>.</summary>
        public void Clear() { _inner.Clear(); }
        /// <summary>Equivalent to the same method in <see cref="Dictionary{TKey,TValue}"/>.</summary>
        public bool ContainsKey(TKey key) { return _inner.ContainsKey(key); }
        /// <summary>Equivalent to the same method in <see cref="Dictionary{TKey,TValue}"/>.</summary>
        public bool ContainsValue(TValue value) { return _inner.ContainsValue(value); }
        /// <summary>Equivalent to the same method in <see cref="Dictionary{TKey,TValue}"/>.</summary>
        public Dictionary<TKey, TValue>.Enumerator GetEnumerator() { return _inner.GetEnumerator(); }
        /// <summary>Equivalent to the same method in <see cref="Dictionary{TKey,TValue}"/>.</summary>
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context) { _inner.GetObjectData(info, context); }
        /// <summary>Equivalent to the same method in <see cref="Dictionary{TKey,TValue}"/>.</summary>
        public virtual void OnDeserialization(object sender) { _inner.OnDeserialization(sender); }
        /// <summary>Equivalent to the same method in <see cref="Dictionary{TKey,TValue}"/>.</summary>
        public bool Remove(TKey key) { return _inner.Remove(key); }
        /// <summary>Equivalent to the same method in <see cref="Dictionary{TKey,TValue}"/>.</summary>
        public bool TryGetValue(TKey key, out TValue value) { value = this[key]; return true; }

        ICollection<TKey> IDictionary<TKey, TValue>.Keys { get { return ((IDictionary<TKey, TValue>) _inner).Keys; } }
        ICollection<TValue> IDictionary<TKey, TValue>.Values { get { return ((IDictionary<TKey, TValue>) _inner).Values; } }
        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item) { ((ICollection<KeyValuePair<TKey, TValue>>) _inner).Add(item); }
        void ICollection<KeyValuePair<TKey, TValue>>.Clear() { ((ICollection<KeyValuePair<TKey, TValue>>) _inner).Clear(); }
        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item) { return ((ICollection<KeyValuePair<TKey, TValue>>) _inner).Contains(item); }
        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) { ((ICollection<KeyValuePair<TKey, TValue>>) _inner).CopyTo(array, arrayIndex); }
        int ICollection<KeyValuePair<TKey, TValue>>.Count { get { return ((ICollection<KeyValuePair<TKey, TValue>>) _inner).Count; } }
        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly { get { return ((ICollection<KeyValuePair<TKey, TValue>>) _inner).IsReadOnly; } }
        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item) { return ((ICollection<KeyValuePair<TKey, TValue>>) _inner).Remove(item); }
        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator() { return ((IEnumerable<KeyValuePair<TKey, TValue>>) _inner).GetEnumerator(); }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return ((IEnumerable) _inner).GetEnumerator(); }
    }

    /// <summary>Implements a two-level <see cref="AutoDictionary{TKey, TValue}"/>.</summary>
    public class AutoDictionary<TKey1, TKey2, TValue> : AutoDictionary<TKey1, AutoDictionary<TKey2, TValue>>
    {
        /// <summary>Constructor.</summary>
        public AutoDictionary(Func<TKey1, TKey2, TValue> initializer = null)
            : base(key1 => new AutoDictionary<TKey2, TValue>(key2 => initializer == null ? default(TValue) : initializer(key1, key2)))
        { }

        /// <summary>Constructor.</summary>
        public AutoDictionary(IEqualityComparer<TKey1> comparer1, IEqualityComparer<TKey2> comparer2, Func<TKey1, TKey2, TValue> initializer = null)
            : base(comparer1, key1 => new AutoDictionary<TKey2, TValue>(comparer2, key2 => initializer == null ? default(TValue) : initializer(key1, key2)))
        { }
    }

    /// <summary>Implements a three-level <see cref="AutoDictionary{TKey, TValue}"/>.</summary>
    public class AutoDictionary<TKey1, TKey2, TKey3, TValue> : AutoDictionary<TKey1, AutoDictionary<TKey2, AutoDictionary<TKey3, TValue>>>
    {
        /// <summary>Constructor.</summary>
        public AutoDictionary(Func<TKey1, TKey2, TKey3, TValue> initializer = null)
            : base(key1 => new AutoDictionary<TKey2, AutoDictionary<TKey3, TValue>>(key2 => new AutoDictionary<TKey3, TValue>(key3 => initializer == null ? default(TValue) : initializer(key1, key2, key3))))
        { }

        /// <summary>Constructor.</summary>
        public AutoDictionary(IEqualityComparer<TKey1> comparer1, IEqualityComparer<TKey2> comparer2, IEqualityComparer<TKey3> comparer3, Func<TKey1, TKey2, TKey3, TValue> initializer = null)
            : base(comparer1, key1 => new AutoDictionary<TKey2, AutoDictionary<TKey3, TValue>>(comparer2, key2 => new AutoDictionary<TKey3, TValue>(comparer3, key3 => initializer == null ? default(TValue) : initializer(key1, key2, key3))))
        { }
    }
}
