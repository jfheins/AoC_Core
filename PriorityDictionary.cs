using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Core
{
    public class PriorityDictionary<TKey, TValue> : IDictionary<TKey, TValue>
        where TKey : notnull
    {
        protected Dictionary<TKey, TValue> _dict = new Dictionary<TKey, TValue>();
        protected C5.IntervalHeap<TValue> _heap;

        public PriorityDictionary(IComparer<TValue> priorityComparer, IEqualityComparer<TValue> equalityComparer)
        {
            _heap = new C5.IntervalHeap<TValue>(priorityComparer);
        }

        public TValue this[TKey key]
        {
            get => _dict[key];

            set
            {
                if (_dict.ContainsKey(key))
                {
                    //_ = _heap.Replace(existing.Handle, newNode);
                    //newNode.Handle = existing.Handle;
                    //openSet[newNode.Item] = newNode;
                }
            }
        }

        public ICollection<TKey> Keys => _dict.Keys;

        public ICollection<TValue> Values => _dict.Values;

        public int Count => _dict.Count;
        public bool IsReadOnly => ((IDictionary<TKey, TValue>)_dict).IsReadOnly;
        public void Add(TKey key, TValue value) => _dict.Add(key, value);
        public void Clear() => _dict.Clear();
        public bool Contains(KeyValuePair<TKey, TValue> item) => ((IDictionary<TKey, TValue>)_dict).Contains(item);
        public bool ContainsKey(TKey key) => _dict.ContainsKey(key);
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => ((IDictionary<TKey, TValue>)_dict).CopyTo(array, arrayIndex);
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _dict.GetEnumerator();
        public bool Remove(TKey key) => _dict.Remove(key);

        public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value) => _dict.TryGetValue(key, out value);
        IEnumerator IEnumerable.GetEnumerator() => _dict.GetEnumerator();

#pragma warning disable CS8597 // Thrown value may be null.
        public bool Remove(KeyValuePair<TKey, TValue> item) => throw null;
        public void Add(KeyValuePair<TKey, TValue> item) => throw null;
#pragma warning restore CS8597 // Thrown value may be null.
    }
}
