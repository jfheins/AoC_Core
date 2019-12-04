using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Core
{
    /// <summary>
    /// Wie ein Dictionary, aber es kann in O(1) der kleinste Wert extrahiert werden.
    /// </summary>
    public sealed class PriorityDictionary<TKey, TValue> : IDictionary<TKey, TValue>
        where TKey : notnull
    {
        private readonly C5.IntervalHeap<KeyValuePair<TKey, TValue>> _heap;
        private readonly Dictionary<TKey, C5.IPriorityQueueHandle<KeyValuePair<TKey, TValue>>> _handles;
        private readonly Dictionary<TKey, TValue> _dict;


        public PriorityDictionary()
        {
            _heap = new C5.IntervalHeap<KeyValuePair<TKey, TValue>>(new KvpByValueComparer());
            _handles = new Dictionary<TKey, C5.IPriorityQueueHandle<KeyValuePair<TKey, TValue>>>();
            _dict = new Dictionary<TKey, TValue>();
        }
        public PriorityDictionary(IComparer<TValue> priorityComparer, IEqualityComparer<TKey>? equalityComparer = null)
        {
            _heap = new C5.IntervalHeap<KeyValuePair<TKey, TValue>>(new KvpByValueComparer(priorityComparer));
            _handles = new Dictionary<TKey, C5.IPriorityQueueHandle<KeyValuePair<TKey, TValue>>>(equalityComparer);
            _dict = new Dictionary<TKey, TValue>(equalityComparer);
        }
        public PriorityDictionary(Comparison<TValue> priorityComparer, IEqualityComparer<TKey>? equalityComparer = null)
            : this(Comparer<TValue>.Create(priorityComparer), equalityComparer)
        {
        }

        public KeyValuePair<TKey, TValue> PopMin()
        {
            var item = _heap.DeleteMin();
            _ = _handles.Remove(item.Key);
            _ = _dict.Remove(item.Key);
            return item;
        }

        public KeyValuePair<TKey, TValue> PeekMin() => _heap.FindMin();

        public KeyValuePair<TKey, TValue> PopMax()
        {
            var item = _heap.DeleteMax();
            _ = _handles.Remove(item.Key);
            _ = _dict.Remove(item.Key);
            return item;
        }

        public KeyValuePair<TKey, TValue> PeekMax() => _heap.FindMax();


        public TValue this[TKey key]
        {
            get => _dict[key];

            set
            {
                if (_handles.ContainsKey(key))
                {
                    _dict[key] = value;
                    _ = _heap.Replace(_handles[key], KeyValuePair.Create(key, value));
                }
                else
                {
                    Add(key, value);
                }
            }
        }

        public ICollection<TKey> Keys => _dict.Keys;

        public ICollection<TValue> Values => _dict.Values;

        public int Count => _dict.Count;
        public bool IsReadOnly => ((IDictionary<TKey, TValue>)_dict).IsReadOnly;
        public void Add(TKey key, TValue value)
        {
            C5.IPriorityQueueHandle<KeyValuePair<TKey, TValue>>? handle = null;
            _ = _heap.Add(ref handle, KeyValuePair.Create(key, value));
            _handles.Add(key, handle);
            _dict.Add(key, value);
        }
        public void Clear()
        {
            while (_heap.Count > 0)
                _ = _heap.DeleteMax();
            _handles.Clear();
            _dict.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item) => ((IDictionary<TKey, TValue>)_dict).Contains(item);
        public bool ContainsKey(TKey key) => _dict.ContainsKey(key);
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => ((IDictionary<TKey, TValue>)_dict).CopyTo(array, arrayIndex);
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _dict.GetEnumerator();
        public bool Remove(TKey key)
        {
            _ = _heap.Delete(_handles[key]);
            _ = _handles.Remove(key);
            return _dict.Remove(key);
        }

        public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value) => _dict.TryGetValue(key, out value);
        IEnumerator IEnumerable.GetEnumerator() => _dict.GetEnumerator();

#pragma warning disable CS8597 // Thrown value may be null.
        public bool Remove(KeyValuePair<TKey, TValue> item) => throw null;
        public void Add(KeyValuePair<TKey, TValue> item) => throw null;
#pragma warning restore CS8597 // Thrown value may be null.


        private class KvpByValueComparer : IComparer<KeyValuePair<TKey, TValue>>
        {
            private readonly IComparer<TValue> _priorityComparer;

            public KvpByValueComparer(IComparer<TValue>? priorityComparer = null)
            {
                _priorityComparer = priorityComparer ?? Comparer<TValue>.Default;
            }

            public int Compare([AllowNull] KeyValuePair<TKey, TValue> x, [AllowNull] KeyValuePair<TKey, TValue> y)
                => _priorityComparer.Compare(x.Value, y.Value);
        }
    }
}
