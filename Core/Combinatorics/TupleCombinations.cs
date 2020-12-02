using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using MoreLinq.Extensions;

namespace Core.Combinatorics
{
    public class TupleCombinations2<T> : IEnumerable<ValueTuple<T, T>> where T : notnull
    {
        public IReadOnlyList<T> SourceValues { get; }

        public TupleCombinations2(IReadOnlyList<T> sourceValues)
        {
            if (sourceValues.Count < 2)            
                throw new InvalidOperationException("Source collection must contain at east 2 elements");
            
            SourceValues = sourceValues;
        }

        public IEnumerator<(T, T)> GetEnumerator()
        {
            return new Enumerator(SourceValues);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(SourceValues);
        }

        private struct Enumerator : IEnumerator<ValueTuple<T, T>> 
        {
            private readonly IReadOnlyList<T> values;
            private int IndexA;
            private int IndexB;

            public Enumerator(IReadOnlyList<T> values) : this()
            {
                this.values = values;
                IndexA = 0;
                IndexB = 0;
            }

            public (T, T) Current => (values[IndexA], values[IndexB]);

            object IEnumerator.Current => Current;

            public void Dispose() { }

            public bool MoveNext()
            {
                if (IndexB < values.Count - 1)
                {
                    IndexB++;
                    return true;
                }
                else if (IndexA < values.Count - 2)
                {
                    IndexA++;
                    IndexB = IndexA + 1;
                    return true;
                }
                return false;
            }

            public void Reset()
            {
                IndexA = 0;
                IndexB = 0;
            }
        }
    }

    public class TupleCombinations3<T> : IEnumerable<ValueTuple<T, T, T>> where T : notnull
    {
        public IReadOnlyList<T> SourceValues { get; }

        public TupleCombinations3(IReadOnlyList<T> sourceValues)
        {
            if (sourceValues.Count < 3)
                throw new InvalidOperationException("Source collection must contain at least 3 elements");

            SourceValues = sourceValues;
        }

        public IEnumerator<(T, T, T)> GetEnumerator()
        {
            return new Enumerator(SourceValues);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(SourceValues);
        }

        private struct Enumerator : IEnumerator<ValueTuple<T, T, T>>
        {
            private readonly IReadOnlyList<T> values;
            private int IndexA;
            private int IndexB;
            private int IndexC;

            public Enumerator(IReadOnlyList<T> values) : this()
            {
                this.values = values;
                IndexA = 0;
                IndexB = 1;
                IndexC = 1;
            }

            public (T, T, T) Current => (values[IndexA], values[IndexB], values[IndexC]);

            object IEnumerator.Current => Current;

            public void Dispose() { }

            public bool MoveNext()
            {
                var maxc = values.Count - 1;
                var maxb = values.Count - 2;
                var maxa = values.Count - 3;

                if (IndexC < maxc)
                {
                    IndexC++;
                    return true;
                }
                else if (IndexB < maxb)
                {
                    IndexB++;
                    IndexC = IndexB + 1;
                    return true;
                }
                else if (IndexA < maxa)
                {
                    IndexA++;
                    IndexB = IndexA + 1;
                    IndexC = IndexA + 2;
                    return true;
                }
                return false;
            }

            public void Reset()
            {
                IndexA = 0;
                IndexB = 1;
                IndexC = 1;
            }
        }
    }


    /// <summary>
    /// This will mutate the internal array in place and return it many times,
    /// so you must not store a the array reference.
    /// Either filter in the enumeration immediately or copy the arrays.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class FastCombinations<T> : IEnumerable<ReadOnlyCollection<T>> where T : notnull
    {
        public IReadOnlyList<T> SourceValues { get; }
        public int CombinationLength { get; }

        public FastCombinations(IReadOnlyList<T> sourceValues, int combinationLength)
        {
            if (sourceValues.Count < 3)
                throw new InvalidOperationException("Source collection must contain at least 3 elements");

            SourceValues = sourceValues;
            CombinationLength = combinationLength;
        }

        public IEnumerator<ReadOnlyCollection<T>> GetEnumerator()
        {
            return new Enumerator(SourceValues, CombinationLength);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(SourceValues, CombinationLength);
        }

        private struct Enumerator : IEnumerator<ReadOnlyCollection<T>>
        {
            private readonly IReadOnlyList<T> values;
            private readonly int combiLength;
            private int[] indices;
            private T[] current;

            public Enumerator(IReadOnlyList<T> values, int combiLength) : this()
            {
                this.values = values;
                this.combiLength = combiLength;
                Reset();
                Current = Array.AsReadOnly(current);
            }

            public ReadOnlyCollection<T> Current { get;  }

            object IEnumerator.Current => Current;

            public void Dispose() { }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void ComputeCurrent(int start)
            {
                for (int i = start; i < current.Length; i++)
                {
                    current[i] = values[indices[i]];
                }
            }

            public bool MoveNext()
            {
                for (int i = combiLength - 1; i >= 0; i--)
                {
                    var max = values.Count - combiLength + i;
                    if (indices[i] < max)
                    {
                        indices[i]++;
                        for (int j = i + 1; j < combiLength; j++)
                        {
                            indices[j] = indices[j - 1] + 1;
                        }
                        ComputeCurrent(i);
                        return true;
                    }
                }
                return false;
            }

            public void Reset()
            {
                indices = Enumerable.Range(0, combiLength).ToArray();
                indices[combiLength - 1]--;
                current = new T[combiLength];
                ComputeCurrent(0);
            }
        }
    }
}
