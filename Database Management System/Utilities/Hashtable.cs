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
        private readonly LinkedList<KeyValuePair<TKey, TValue>>[] buckets;

        public HashTable(int size = 16)
        {
            this.size = size;
            buckets = new LinkedList<KeyValuePair<TKey, TValue>>[size];
        }

        private int GetBucketIndex(TKey key)
        {
            return Math.Abs(key.GetHashCode()) % size;
        }

        public void Add(TKey key, TValue value)
        {
            int index = GetBucketIndex(key);
            if (buckets[index] == null)
            {
                buckets[index] = new LinkedList<KeyValuePair<TKey, TValue>>();
            }

            foreach (var pair in buckets[index])
            {
                if (pair.Key.Equals(key))
                {
                    throw new ArgumentException($"Key '{key}' already exists in the hashtable.");
                }
            }

            buckets[index].AddLast(new KeyValuePair<TKey, TValue>(key, value));
        }

        public TValue Get(TKey key)
        {
            int index = GetBucketIndex(key);
            if (buckets[index] != null)
            {
                foreach (var pair in buckets[index])
                {
                    if (pair.Key.Equals(key))
                    {
                        return pair.Value;
                    }
                }
            }

            throw new KeyNotFoundException($"Key '{key}' not found in the hashtable.");
        }

        public bool ContainsKey(TKey key)
        {
            int index = GetBucketIndex(key);
            if (buckets[index] != null)
            {
                foreach (var pair in buckets[index])
                {
                    if (pair.Key.Equals(key))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public void Remove(TKey key)
        {
            int index = GetBucketIndex(key);
            if (buckets[index] != null)
            {
                var node = buckets[index].First;
                while (node != null)
                {
                    if (node.Value.Key.Equals(key))
                    {
                        buckets[index].Remove(node);
                        return;
                    }
                    node = node.Next;
                }
            }

            throw new KeyNotFoundException($"Key '{key}' not found in the hashtable.");
        }
    }
}
