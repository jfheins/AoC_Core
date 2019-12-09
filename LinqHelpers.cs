using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

namespace Core
{
    public static class LinqHelpers
    {
        /// <summary>
        ///     Verpackt den angegebenen Wert in eine Enumeration mit einem Element.
        /// </summary>
        /// <typeparam name="T">Ein beliebiger Typ.</typeparam>
        /// <param name="item">Der Wert, der verpackt werden soll.</param>
        /// <returns>Eine Enumeration, die genau einen Wert enthält.</returns>
        public static IEnumerable<T> ToEnumerable<T>(this T item)
        {
            yield return item;
        }

        public static IEnumerable<int> IndexWhere<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            return source.Select((value, index) => new { value, index })
                .Where(x => predicate(x.value))
                .Select(x => x.index);
        }

        public static IEnumerable<T> ExceptFor<T>(this IEnumerable<T> source, T exception)
        {
            return source.Where(x => !x.Equals(exception));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1062:Validate arguments of public methods")]
        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, Func<TKey, TValue> factory) where TKey : notnull
        {
            if (dict.TryGetValue(key, out var data))
                return data;
            else
                return dict[key] = factory(key);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1062:Validate arguments of public methods")]
        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue defaultValue) where TKey : notnull
        {
            if (dict.TryGetValue(key, out var data))
                return data;
            else
                return dict[key] = defaultValue;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1062:Validate arguments of public methods")]
        public static IEnumerable<TValue> GetOrEmpty<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key) where TKey : notnull
        {
            if (dict.TryGetValue(key, out var data))
                yield return data;
        }

        public static int[] ParseInts(this string str, int? count = null)
        {
            var regex = new Regex(@"([-+]?[0-9]+)");
            var matches = regex.Matches(str);
            if (count != null)
            {
                Debug.Assert(matches.Count == count);
            }

            return matches.Select(match => int.Parse(match.Value)).ToArray();
        }

        public static IEnumerable<ValueTuple<T1, T2>> CartesianProduct<T1, T2>(this IEnumerable<T1> a,
            IEnumerable<T2> b)
        {
            if (!(b is ICollection<T2>))
            {
                b = b.ToList();
            }

            return a.SelectMany(x => b, (x, y) => (x, y));
        }

        public static (T min, T max) MinMax<T>(this ICollection<T> source)
        {
            return (source.Min(), source.Max());
        }

        public static (TResult min, TResult max) MinMax<T, TResult>(this ICollection<T> source,
            Func<T, TResult> selector)
        {
            return (source.Min(selector), source.Max(selector));
        }

        public static int Diff(this ValueTuple<int, int> pair)
        {
            return pair.Item2 - pair.Item1;
        }

        public static long Diff(this ValueTuple<long, long> pair)
        {
            return pair.Item2 - pair.Item1;
        }

        public static double Diff(this ValueTuple<double, double> pair)
        {
            return pair.Item2 - pair.Item1;
        }

        public static bool AreAllEqual<T>(this IEnumerable<T> source) where T : IEquatable<T>
        {
            var first = source.First();
            return source.All(x => x.Equals(first));
        }

        public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T> source)
        {
            return source.Where(x => x != null);
        }

        /// <summary>
        ///     Liefert zu einer Enumeration alle Paare zurück. Eine Enumeration mit n Elementen hat genau n-1 Paare.
        ///     Die Quelle wird nur einmal durchlaufen. Für jedes Paar wird ein neues Tupel generiert.
        ///     Item1 ist stets das Element, dass in der Quelle zuerst vorkommt.
        /// </summary>
        /// <param name="source">Die Quelle, die paarweise enumeriert werden soll.</param>
        /// <returns>
        ///     Eine Enumeration mit n-1 überschneidenden Tupeln. Gibt eine leere Enumeration zurück, wenn die Quelle aus
        ///     weniger als zwei Elmenten besteht.
        /// </returns>
        public static IEnumerable<ValueTuple<T, T>> PairwiseWithOverlap<T>(this IEnumerable<T> source)
        {
            using (var it = source.GetEnumerator())
            {
                if (!it.MoveNext())
                {
                    yield break;
                }

                var previous = it.Current;

                while (it.MoveNext())
                {
                    yield return ValueTuple.Create(previous, previous = it.Current);
                }
            }
        }

        public static IEnumerable<ValueTuple<T, T>> Pairwise<T>(this IEnumerable<T> source)
        {
            var isPair = false;
            var tempItem = default(T);
            foreach (var item in source)
            {
                if (isPair)
                {
                    yield return ValueTuple.Create(tempItem, item);
                    isPair = false;
                }
                else
                {
                    tempItem = item;
                    isPair = true;
                }
            }
        }

        public static IEnumerable<IEnumerable<T>> Chunks<T>(this IEnumerable<T> enumerable)
        {
            var chunk = new List<T>();
            var reference = default(T);

            using var e = enumerable.GetEnumerator();
            while (e.MoveNext())
            {
                if (chunk.Count == 0)
                {
                    reference = e.Current;
                    chunk.Add(e.Current);
                }
                else if (e.Current!.Equals(reference))
                {
                    chunk.Add(e.Current);
                }
                else
                {
                    yield return chunk;
                    chunk = new List<T>();
                    reference = e.Current;
                    chunk.Add(e.Current);
                }
            }
            if (chunk.Count > 0)
                yield return chunk;
        }

        // https://stackoverflow.com/questions/419019/split-list-into-sublists-with-linq/20953521#20953521
        public static IEnumerable<IEnumerable<T>> Chunks<T>(this IEnumerable<T> enumerable,
            int chunkSize)
        {
            if (chunkSize < 1)
            {
                throw new ArgumentException("chunkSize must be positive");
            }

            using (var e = enumerable.GetEnumerator())
            {
                while (e.MoveNext())
                {
                    var remaining = chunkSize; // elements remaining in the current chunk
                    // ReSharper disable once AccessToDisposedClosure
                    var innerMoveNext = new Func<bool>(() => --remaining > 0 && e.MoveNext());

                    yield return e.GetChunk(innerMoveNext);
                    while (innerMoveNext())
                    {
                        /* discard elements skipped by inner iterator */
                    }
                }
            }
        }

        private static IEnumerable<T> GetChunk<T>(this IEnumerator<T> e,
            Func<bool> innerMoveNext)
        {
            do
            {
                yield return e.Current;
            } while (innerMoveNext());
        }
    }
}
