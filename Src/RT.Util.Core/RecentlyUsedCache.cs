using System.Collections.Generic;

namespace RT.Util.Collections
{
    /// <summary>
    ///     Implements a key-value store which remembers which keys were used more recently than others, and automatically
    ///     trims the older entries once a threshold is reached. Lookups are O(1) and are comparable in speed to a
    ///     Dictionary`2. So are additions, except when a trim is triggered.</summary>
    class RecentlyUsedCache<TKey, TValue>
    {
        private struct Entry
        {
            public long LastUsedAt { get; private set; }
            public TValue Value { get; private set; }

            public Entry(long lastUsedAt, TValue value)
                : this()
            {
                LastUsedAt = lastUsedAt;
                Value = value;
            }
        }

        private long _current, _oldest;
        private Dictionary<TKey, Entry> _cache = new Dictionary<TKey, Entry>();
        private int _trimAt, _trimTo;

        /// <summary>
        ///     Constructor.</summary>
        /// <param name="trimAt">
        ///     Whenever the cache has this many entries, a trim will be triggered.</param>
        /// <param name="trimTo">
        ///     The minimum number of most recently used entries to remain in the cache after trimming. The actual number of
        ///     entries will be somewhere between <paramref name="trimTo"/> and 2 * <paramref name="trimTo"/>.</param>
        public RecentlyUsedCache(int trimAt = 15000, int trimTo = 1000)
        {
            _trimAt = trimAt;
            _trimTo = trimTo;
        }

        /// <summary>
        ///     Gets a value associated with the specified key, and records it as recently used.</summary>
        /// <param name="key">
        ///     The key to retrieve.</param>
        /// <param name="value">
        ///     Receives the value in case of successful lookup, or <c>default(TValue)</c> if the specified key is not
        ///     currently stored.</param>
        /// <returns>
        ///     True if the key was found, false otherwise.</returns>
        public bool Retrieve(TKey key, out TValue value)
        {
            Entry entry;
            if (_cache.TryGetValue(key, out entry))
            {
                value = entry.Value;
                _current++;
                _cache[key] = new Entry(_current, value);
                return true;
            }
            else
            {
                value = default(TValue);
                return false;
            }
        }

        /// <summary>
        ///     Stores the specified key/value pair in the cache and records the key as recently used. Triggers a
        ///     (comparatively slow) trim operation if the total number of entries exceed the threshold specified when the
        ///     cache was instantiated.</summary>
        public void Store(TKey key, TValue value)
        {
            _current++;
            _cache[key] = new Entry(_current, value);
            if (_cache.Count > _trimAt)
                trim();
        }

        private void trim()
        {
            long min = _oldest;
            long max = _current;
            long thresh = (long) ((min + max) * ((_trimAt - 2 * _trimTo) / (double) _trimAt)); // trim a lot more the first time round
            var toRemove = new List<TKey>();
            while (_cache.Count > _trimTo * 2)
            {
                // See how many this would preserve
                long preserved = 0;
                foreach (var val in _cache.Values)
                {
                    if (val.LastUsedAt >= thresh)
                    {
                        preserved++;
                        if (preserved >= _trimTo)
                            break; // we'll definitely not be removing too much
                    }
                }
                // Remove if the threshold is acceptable
                if (preserved >= _trimTo)
                {
                    toRemove.Clear();
                    _oldest = long.MaxValue;
                    foreach (var kvp in _cache)
                    {
                        if (kvp.Value.LastUsedAt < thresh)
                            toRemove.Add(kvp.Key);
                        else if (kvp.Value.LastUsedAt < _oldest)
                            _oldest = kvp.Value.LastUsedAt;
                    }
                    foreach (var key in toRemove)
                        _cache.Remove(key);
                    min = _oldest;
                }
                else
                    max = thresh;
                thresh = (min + max) / 2;
            }
        }
    }
}
