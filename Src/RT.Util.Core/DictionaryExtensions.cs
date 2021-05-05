using System;
using System.Collections.Generic;
using System.Linq;

namespace RT.Util.ExtensionMethods
{
    /// <summary>
    ///     Provides extension methods on dictionaries (<see cref="Dictionary{TKey, TValue}"/> and <see
    ///     cref="IDictionary{TKey, TValue}"/>).</summary>
#if EXPORT_UTIL
    public
#endif
    static class DictionaryExtensions
    {
        /// <summary>Determines whether the current HashSet-in-a-Dictionary contains the specified key and value.</summary>
        public static bool Contains<TKey, TValue>(this IDictionary<TKey, HashSet<TValue>> source, TKey key, TValue value)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (key == null)
                throw new ArgumentNullException(nameof(key), "Null values cannot be used for keys in dictionaries.");
            return source.ContainsKey(key) && source[key].Contains(value);
        }

        /// <summary>
        ///     Determines whether the current two-level dictionary contains the specified combination of keys.</summary>
        /// <typeparam name="TKey1">
        ///     Type of the first-level key.</typeparam>
        /// <typeparam name="TKey2">
        ///     Type of the second-level key.</typeparam>
        /// <typeparam name="TValue">
        ///     Type of values in the dictionary.</typeparam>
        /// <param name="source">
        ///     Source dictionary to examine.</param>
        /// <param name="key1">
        ///     The first key to check for.</param>
        /// <param name="key2">
        ///     The second key to check for.</param>
        public static bool ContainsKeys<TKey1, TKey2, TValue>(this IDictionary<TKey1, Dictionary<TKey2, TValue>> source, TKey1 key1, TKey2 key2) =>
            source == null ? throw new ArgumentNullException(nameof(source)) : !source.TryGetValue(key1, out var dic) ? false : dic.ContainsKey(key2);

        /// <summary>
        ///     Gets the value associated with the specified combination of keys.</summary>
        /// <typeparam name="TKey1">
        ///     Type of the first-level key.</typeparam>
        /// <typeparam name="TKey2">
        ///     Type of the second-level key.</typeparam>
        /// <typeparam name="TValue">
        ///     Type of values in the dictionary.</typeparam>
        /// <param name="source">
        ///     Source dictionary to examine.</param>
        /// <param name="key1">
        ///     The first key to check for.</param>
        /// <param name="key2">
        ///     The second key to check for.</param>
        /// <param name="value">
        ///     When this method returns, the value associated with the specified keys, if the keys are found; otherwise, the
        ///     default value for the type of the <paramref name="value"/> parameter. This parameter is passed uninitialized.</param>
        /// <returns>
        ///     <c>true</c> if the two-level dictionary contains an element with the specified combination of keys; otherwise,
        ///     <c>false</c>.</returns>
        public static bool TryGetValue<TKey1, TKey2, TValue>(this IDictionary<TKey1, Dictionary<TKey2, TValue>> source, TKey1 key1, TKey2 key2, out TValue value)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            value = default(TValue);
            return source.TryGetValue(key1, out var dic) ? dic.TryGetValue(key2, out value) : false;
        }

        /// <summary>
        ///     Compares two dictionaries for equality, member-wise. Two dictionaries are equal if they contain all the same
        ///     key-value pairs.</summary>
        public static bool DictionaryEqual<TK, TV>(this IDictionary<TK, TV> dictA, IDictionary<TK, TV> dictB)
            where TV : IEquatable<TV>
        {
            if (dictA == null)
                throw new ArgumentNullException(nameof(dictA));
            if (dictB == null)
                throw new ArgumentNullException(nameof(dictB));
            if (dictA.Count != dictB.Count)
                return false;
            foreach (var key in dictA.Keys)
            {
                if (!dictB.ContainsKey(key))
                    return false;
                if (!dictA[key].Equals(dictB[key]))
                    return false;
            }
            return true;
        }

        /// <summary>
        ///     Gets a value from a dictionary by key. If the key does not exist in the dictionary, the default value is
        ///     returned instead.</summary>
        /// <param name="dict">
        ///     Dictionary to operate on.</param>
        /// <param name="key">
        ///     Key to look up.</param>
        /// <param name="defaultVal">
        ///     Value to return if <paramref name="key"/> is not contained in the dictionary.</param>
        public static TValue Get<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue defaultVal)
        {
            if (dict == null)
                throw new ArgumentNullException(nameof(dict));
            if (key == null)
                throw new ArgumentNullException(nameof(key), "Null values cannot be used for keys in dictionaries.");
            return dict.TryGetValue(key, out var value) ? value : defaultVal;
        }

        /// <summary>
        ///     Gets a value from a dictionary by key. If the key does not exist in the dictionary, the default value is
        ///     returned instead.</summary>
        /// <param name="dict">
        ///     Dictionary to operate on.</param>
        /// <param name="key">
        ///     Key to look up.</param>
        /// <param name="defaultVal">
        ///     Value to return if <paramref name="key"/> is not contained in the dictionary.</param>
        public static TValue? Get<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue? defaultVal = null) where TValue : struct
        {
            if (dict == null)
                throw new ArgumentNullException(nameof(dict));
            if (key == null)
                throw new ArgumentNullException(nameof(key), "Null values cannot be used for keys in dictionaries.");
            return dict.TryGetValue(key, out var value) ? (TValue?) value : defaultVal;
        }

        /// <summary>
        ///     Gets a value from a two-level dictionary by key. If the keys don’t exist in the dictionary, the default value
        ///     is returned instead.</summary>
        /// <param name="dict">
        ///     Dictionary to operate on.</param>
        /// <param name="key1">
        ///     Key to look up in the first level.</param>
        /// <param name="key2">
        ///     Key to look up in the second level.</param>
        /// <param name="defaultVal">
        ///     Value to return if key1 or key2 is not contained in the relevant dictionary.</param>
        public static TValue Get<TKey1, TKey2, TValue>(this IDictionary<TKey1, Dictionary<TKey2, TValue>> dict, TKey1 key1, TKey2 key2, TValue defaultVal)
        {
            if (dict == null)
                throw new ArgumentNullException(nameof(dict));
            if (key1 == null)
                throw new ArgumentNullException(nameof(key1), "Null values cannot be used for keys in dictionaries.");
            if (key2 == null)
                throw new ArgumentNullException(nameof(key2), "Null values cannot be used for keys in dictionaries.");

            return dict.TryGetValue(key1, out var innerDic) && innerDic.TryGetValue(key2, out var value) ? value : defaultVal;
        }

        /// <summary>
        ///     Removes all entries from a dictionary that satisfy a specified predicate.</summary>
        /// <typeparam name="TKey">
        ///     Type of the keys in the dictionary.</typeparam>
        /// <typeparam name="TVal">
        ///     Type of the values in the dictionary.</typeparam>
        /// <param name="dict">
        ///     Dictionary to operate on.</param>
        /// <param name="predicate">
        ///     Specifies a predicate that determines which entries should be removed from the dictionary.</param>
        public static void RemoveAll<TKey, TVal>(this IDictionary<TKey, TVal> dict, Func<KeyValuePair<TKey, TVal>, bool> predicate)
        {
            foreach (var kvp in dict.Where(kvp => predicate(kvp)).ToArray())
                dict.Remove(kvp.Key);
        }

        /// <summary>
        ///     Removes all entries from a dictionary whose keys satisfy a specified predicate.</summary>
        /// <typeparam name="TKey">
        ///     Type of the keys in the dictionary.</typeparam>
        /// <typeparam name="TVal">
        ///     Type of the values in the dictionary.</typeparam>
        /// <param name="dict">
        ///     Dictionary to operate on.</param>
        /// <param name="predicate">
        ///     Specifies a predicate that determines which entries should be removed from the dictionary.</param>
        public static void RemoveAllByKey<TKey, TVal>(this IDictionary<TKey, TVal> dict, Func<TKey, bool> predicate)
        {
            foreach (var kvp in dict.Where(kvp => predicate(kvp.Key)).ToArray())
                dict.Remove(kvp.Key);
        }

        /// <summary>
        ///     Removes all entries from a dictionary whose values satisfy a specified predicate.</summary>
        /// <typeparam name="TKey">
        ///     Type of the keys in the dictionary.</typeparam>
        /// <typeparam name="TVal">
        ///     Type of the values in the dictionary.</typeparam>
        /// <param name="dict">
        ///     Dictionary to operate on.</param>
        /// <param name="predicate">
        ///     Specifies a predicate that determines which entries should be removed from the dictionary.</param>
        public static void RemoveAllByValue<TKey, TVal>(this IDictionary<TKey, TVal> dict, Func<TVal, bool> predicate)
        {
            foreach (var kvp in dict.Where(kvp => predicate(kvp.Value)).ToArray())
                dict.Remove(kvp.Key);
        }

        /// <summary>
        ///     Creates a new dictionary containing the union of the key/value pairs contained in the specified dictionaries.
        ///     Keys in <paramref name="second"/> overwrite keys in <paramref name="first"/>.</summary>
        public static IDictionary<TKey, TValue> CopyMerge<TKey, TValue>(this IDictionary<TKey, TValue> first, IDictionary<TKey, TValue> second)
        {
            if (first == null)
                throw new ArgumentNullException(nameof(first));
            if (second == null)
                throw new ArgumentNullException(nameof(second));
            Dictionary<TKey, TValue> dict = new Dictionary<TKey, TValue>(first);
            foreach (var kvp in second)
                dict.Add(kvp.Key, kvp.Value);
            return dict;
        }

        /// <summary>
        ///     Adds an element to a List&lt;V&gt; stored in the current IDictionary&lt;K, List&lt;V&gt;&gt;. If the specified
        ///     key does not exist in the current IDictionary, a new List is created.</summary>
        /// <typeparam name="K">
        ///     Type of the key of the IDictionary.</typeparam>
        /// <typeparam name="V">
        ///     Type of the values in the Lists.</typeparam>
        /// <param name="dic">
        ///     IDictionary to operate on.</param>
        /// <param name="key">
        ///     Key at which the list is located in the IDictionary.</param>
        /// <param name="value">
        ///     Value to add to the List located at the specified Key.</param>
        public static void AddSafe<K, V>(this IDictionary<K, List<V>> dic, K key, V value)
        {
            if (dic == null)
                throw new ArgumentNullException(nameof(dic));
            if (key == null)
                throw new ArgumentNullException(nameof(key), "Null values cannot be used for keys in dictionaries.");
            if (!dic.ContainsKey(key))
                dic[key] = new List<V>();
            dic[key].Add(value);
        }

        /// <summary>
        ///     Adds an element to a HashSet&lt;V&gt; stored in the current IDictionary&lt;K, HashSet&lt;V&gt;&gt;. If the
        ///     specified key does not exist in the current IDictionary, a new HashSet is created.</summary>
        /// <typeparam name="K">
        ///     Type of the key of the IDictionary.</typeparam>
        /// <typeparam name="V">
        ///     Type of the values in the HashSets.</typeparam>
        /// <param name="dic">
        ///     IDictionary to operate on.</param>
        /// <param name="key">
        ///     Key at which the HashSet is located in the IDictionary.</param>
        /// <param name="value">
        ///     Value to add to the HashSet located at the specified Key.</param>
        public static bool AddSafe<K, V>(this IDictionary<K, HashSet<V>> dic, K key, V value)
        {
            if (dic == null)
                throw new ArgumentNullException(nameof(dic));
            if (key == null)
                throw new ArgumentNullException(nameof(key), "Null values cannot be used for keys in dictionaries.");
            if (!dic.ContainsKey(key))
                dic[key] = new HashSet<V>();
            return dic[key].Add(value);
        }

        /// <summary>
        ///     Adds an element to a two-level Dictionary&lt;,&gt;. If the specified key does not exist in the outer
        ///     Dictionary, a new Dictionary is created.</summary>
        /// <typeparam name="K1">
        ///     Type of the key of the outer Dictionary.</typeparam>
        /// <typeparam name="K2">
        ///     Type of the key of the inner Dictionary.</typeparam>
        /// <typeparam name="V">
        ///     Type of the values in the inner Dictionary.</typeparam>
        /// <param name="dic">
        ///     Dictionary to operate on.</param>
        /// <param name="key1">
        ///     Key at which the inner Dictionary is located in the outer Dictionary.</param>
        /// <param name="key2">
        ///     Key at which the value is located in the inner Dictionary.</param>
        /// <param name="value">
        ///     Value to add to the inner Dictionary.</param>
        /// <param name="comparer">
        ///     Optional equality comparer to pass into the inner dictionary if a new one is created.</param>
        public static void AddSafe<K1, K2, V>(this IDictionary<K1, Dictionary<K2, V>> dic, K1 key1, K2 key2, V value, IEqualityComparer<K2> comparer = null)
        {
            if (dic == null)
                throw new ArgumentNullException(nameof(dic));
            if (key1 == null)
                throw new ArgumentNullException(nameof(key1), "Null values cannot be used for keys in dictionaries.");
            if (key2 == null)
                throw new ArgumentNullException(nameof(key2), "Null values cannot be used for keys in dictionaries.");
            if (!dic.ContainsKey(key1))
                dic[key1] = comparer == null ? new Dictionary<K2, V>() : new Dictionary<K2, V>(comparer);
            dic[key1][key2] = value;
        }

        /// <summary>
        ///     Removes an element from a two-level Dictionary&lt;,&gt;. If this leaves the inner dictionary empty, the key is
        ///     removed from the outer Dictionary.</summary>
        /// <typeparam name="K1">
        ///     Type of the key of the outer Dictionary.</typeparam>
        /// <typeparam name="K2">
        ///     Type of the key of the inner Dictionary.</typeparam>
        /// <typeparam name="V">
        ///     Type of the values in the inner Dictionary.</typeparam>
        /// <param name="dic">
        ///     Dictionary to operate on.</param>
        /// <param name="key1">
        ///     Key at which the inner Dictionary is located in the outer Dictionary.</param>
        /// <param name="key2">
        ///     Key at which the value is located in the inner Dictionary.</param>
        /// <returns>
        ///     A value indicating whether a value was removed or not.</returns>
        public static bool RemoveSafe<K1, K2, V>(this IDictionary<K1, Dictionary<K2, V>> dic, K1 key1, K2 key2)
        {
            if (dic == null)
                throw new ArgumentNullException(nameof(dic));
            if (key1 == null)
                throw new ArgumentNullException(nameof(key1), "Null values cannot be used for keys in dictionaries.");
            if (key2 == null)
                throw new ArgumentNullException(nameof(key2), "Null values cannot be used for keys in dictionaries.");
            if (!dic.TryGetValue(key1, out var inner) || !inner.ContainsKey(key2))
                return false;
            inner.Remove(key2);
            if (inner.Count == 0)
                dic.Remove(key1);
            return true;
        }

        /// <summary>
        ///     Adds an element to a List&lt;V&gt; stored in a two-level Dictionary&lt;,&gt;. If the specified key does not
        ///     exist in the current Dictionary, a new List is created.</summary>
        /// <typeparam name="K1">
        ///     Type of the key of the first-level Dictionary.</typeparam>
        /// <typeparam name="K2">
        ///     Type of the key of the second-level Dictionary.</typeparam>
        /// <typeparam name="V">
        ///     Type of the values in the Lists.</typeparam>
        /// <param name="dic">
        ///     Dictionary to operate on.</param>
        /// <param name="key1">
        ///     Key at which the second-level Dictionary is located in the first-level Dictionary.</param>
        /// <param name="key2">
        ///     Key at which the list is located in the second-level Dictionary.</param>
        /// <param name="value">
        ///     Value to add to the List located at the specified Keys.</param>
        public static void AddSafe<K1, K2, V>(this IDictionary<K1, Dictionary<K2, List<V>>> dic, K1 key1, K2 key2, V value)
        {
            if (dic == null)
                throw new ArgumentNullException(nameof(dic));
            if (key1 == null)
                throw new ArgumentNullException(nameof(key1), "Null values cannot be used for keys in dictionaries.");
            if (key2 == null)
                throw new ArgumentNullException(nameof(key2), "Null values cannot be used for keys in dictionaries.");
            if (!dic.ContainsKey(key1))
                dic[key1] = new Dictionary<K2, List<V>>();
            if (!dic[key1].ContainsKey(key2))
                dic[key1][key2] = new List<V>();
            dic[key1][key2].Add(value);
        }

        /// <summary>
        ///     Increments an integer in an <see cref="IDictionary&lt;K, V&gt;"/> by the specified amount. If the specified
        ///     key does not exist in the current dictionary, the value <paramref name="amount"/> is inserted.</summary>
        /// <typeparam name="K">
        ///     Type of the key of the dictionary.</typeparam>
        /// <param name="dic">
        ///     Dictionary to operate on.</param>
        /// <param name="key">
        ///     Key at which the list is located in the dictionary.</param>
        /// <param name="amount">
        ///     The amount by which to increment the integer.</param>
        /// <returns>
        ///     The new value at the specified key.</returns>
        public static int IncSafe<K>(this IDictionary<K, int> dic, K key, int amount = 1)
        {
            if (dic == null)
                throw new ArgumentNullException(nameof(dic));
            if (key == null)
                throw new ArgumentNullException(nameof(key), "Null values cannot be used for keys in dictionaries.");
            return dic.ContainsKey(key) ? (dic[key] = dic[key] + amount) : (dic[key] = amount);
        }

        /// <summary>
        ///     Removes the first occurrence of an element from a List&lt;V&gt; stored in the current IDictionary&lt;K,
        ///     List&lt;V&gt;&gt;. If this leaves the list stored at the specified key empty, the key is removed from the
        ///     IDictionary. If the key is not in the dictionary to begin with, nothing happens.</summary>
        /// <typeparam name="K">
        ///     Type of the key of the IDictionary.</typeparam>
        /// <typeparam name="V">
        ///     Type of the values in the Lists.</typeparam>
        /// <param name="dic">
        ///     IDictionary to operate on.</param>
        /// <param name="key">
        ///     Key at which the list is located in the IDictionary.</param>
        /// <param name="value">
        ///     Value to add to the List located at the specified Key.</param>
        public static void RemoveSafe<K, V>(this IDictionary<K, List<V>> dic, K key, V value)
        {
            if (dic == null)
                throw new ArgumentNullException(nameof(dic));
            if (key == null)
                throw new ArgumentNullException(nameof(key), "Null values cannot be used for keys in dictionaries.");
            if (dic.ContainsKey(key))
            {
                dic[key].Remove(value);
                if (dic[key].Count == 0)
                    dic.Remove(key);
            }
        }
    }
}
