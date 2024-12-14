using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database_Management_System.Utilities
{
    public class HashTable<TKey, TValue>
    {
        private readonly int size;
        private readonly List<KeyValuePair<TKey, TValue>>[] buckets;

        public HashTable(int size = 16)
        {
            this.size = size;
            buckets = new List<KeyValuePair<TKey, TValue>>[size];
            for (int i = 0; i < size; i++)
            {
                buckets[i] = new List<KeyValuePair<TKey, TValue>>(); // Initialize each bucket as a List
            }
        }

        private int GetBucketIndex(TKey key)
        {
            return Math.Abs(key.GetHashCode()) % size;
        }

        public void Add(TKey key, TValue value)
        {
            int index = GetBucketIndex(key);

            // Check if the key already exists in the current bucket
            foreach (var pair in buckets[index])
            {
                if (pair.Key.Equals(key))
                {
                    throw new ArgumentException($"Key '{key}' already exists in the hashtable.");
                }
            }

            // Add the key-value pair to the correct bucket
            buckets[index].Add(new KeyValuePair<TKey, TValue>(key, value));
        }

        public TValue Get(TKey key)
        {
            int index = GetBucketIndex(key);
            foreach (var pair in buckets[index])
            {
                if (pair.Key.Equals(key))
                {
                    return pair.Value;
                }
            }

            throw new KeyNotFoundException($"Key '{key}' not found in the hashtable.");
        }
        public IEnumerable<TKey> GetKeys()
        {
            var keys = new List<TKey>();
            foreach (var bucket in buckets)
            {
                if (bucket != null)
                {
                    foreach (var pair in bucket)
                    {
                        keys.Add(pair.Key);
                    }
                }
            }
            return keys;
        }

        public bool ContainsKey(TKey key)
        {
            int index = GetBucketIndex(key);
            foreach (var pair in buckets[index])
            {
                if (pair.Key.Equals(key))
                {
                    return true;
                }
            }
            return false;
        }

        public void Remove(TKey key)
        {
            int index = GetBucketIndex(key);
            var bucket = buckets[index];

            var pairToRemove = bucket.FirstOrDefault(pair => pair.Key.Equals(key));
            if (pairToRemove.Equals(default(KeyValuePair<TKey, TValue>)))
            {
                throw new KeyNotFoundException($"Key '{key}' not found in the hashtable.");
            }

            bucket.Remove(pairToRemove);
        }

        // Utility methods to access bucket size and entries
        public int GetSize()
        {
            return size;
        }

        public List<KeyValuePair<TKey, TValue>> GetBucket(int index)
        {
            if (index >= 0 && index < size)
            {
                return buckets[index];
            }
            return null;
        }
    }
}
