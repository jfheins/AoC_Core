using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Core
{
    [SuppressMessage("Naming", "CA1710:Identifiers should have correct suffix", Justification = "Suffix: List")]
    public class NonEmptyList<T> : IList<T>
    {
        public T Head { get; private set; }
        public List<T> Tail { get; } = new List<T>();

        private EqualityComparer<T> _comp = EqualityComparer<T>.Default;

        public NonEmptyList(T head)
        {
            Head = head;
        }
        public NonEmptyList(IEnumerable<T> source)
        {
            Head = source.First();
            Tail = source.Skip(1).ToList();
        }

        public T this[int index]
        {
            get
            {
                return index == 0 ? Head : Tail[index - 1];
            }
            set
            {
                _ = index == 0 ? Head = value : Tail[index - 1] = value;
            }
        }

        public int Count => Tail.Count + 1;
        public bool IsReadOnly => false;

        public void Add(T item)
        {
            Tail.Add(item);
        }

        public void Clear()
        {
            throw new NotSupportedException();
        }

        public bool Contains(T item)
        {
            return _comp.Equals(Head, item) || Tail.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            array[arrayIndex] = Head;
            Tail.CopyTo(array, arrayIndex + 1);
        }

        public IEnumerator<T> GetEnumerator() => new Enumerator(this);

        public int IndexOf(T item)
        {
            return _comp.Equals(Head, item) ? 0 : Tail.IndexOf(item) + 1;
        }

        public void Insert(int index, T item)
        {
            if (index == 0)
            {
                Tail.Insert(0, Head);
                Head = item;
            }
            else
            {
                Tail.Insert(index - 1, item);
            }
        }

        public bool Remove(T item)
        {
            var idx = IndexOf(item);
            if (idx >= 0)
            {
                RemoveAt(idx);
                return true;
            }
            return false;
        }

        public void RemoveAt(int index)
        {
            if ((uint)index >= (uint)Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            if (Tail.Count == 0)
                throw new NotSupportedException("Connot remove the last element!");


            if (index == 0)
            {
                Head = Tail[0];
                Tail.RemoveAt(0);
            }
            else
                Tail.RemoveAt(index - 1);
        }

        IEnumerator IEnumerable.GetEnumerator() => new Enumerator(this);

        public struct Enumerator : IEnumerator<T>, IEnumerator
        {
            private readonly NonEmptyList<T> _list;
            private int _index;
            private T? _current;

            internal Enumerator(NonEmptyList<T> list)
            {
                _list = list;
                _index = 0;
                _current = default;
            }

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                NonEmptyList<T> localList = _list;

                if ((uint)_index < (uint)localList.Count)
                {
                    _current = localList[_index];
                    _index++;
                    return true;
                }

                _index = _list.Count + 1;
                _current = default;
                return false;
            }

            public T Current => _current!;

            object? IEnumerator.Current
            {
                get
                {
                    if (_index == 0 || _index == _list.Count + 1)
                        throw new IndexOutOfRangeException();

                    return Current;
                }
            }

            void IEnumerator.Reset()
            {
                _index = 0;
                _current = default;
            }
        }
    }
}
