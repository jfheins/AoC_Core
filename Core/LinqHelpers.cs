using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Drawing;
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
            return source.Where(x => x is null || !x.Equals(exception));
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
        public static void AddOrModify<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue initValue, Func<TValue, TValue> modifier) where TKey : notnull
        {
            if (dict.TryGetValue(key, out var data))
                dict[key] = modifier(data);
            else
                dict[key] = modifier(initValue);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1062:Validate arguments of public methods")]
        public static IEnumerable<TValue> GetOrEmpty<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key) where TKey : notnull
        {
            if (dict.TryGetValue(key, out var data))
                yield return data;
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

        public static int Product(this IEnumerable<int> source)
        {
            return source.Aggregate(1, (a, b) => a * b);
        }

        public static (T min, T max)? MinMax<T>(this IEnumerable<T> source)
        {
            return source.MinMax(x => x);
        }

        public static (TResult min, TResult max)? MinMax<T, TResult>(this IEnumerable<T> source,
            Func<T, TResult> selector)
        {
            var min = source.Min(selector);
            var max = source.Max(selector);
            return min is null || max is null ? null : (min, max);
        }

        public static (TResult min, TResult max) MinMax<T, TResult>(this NonEmptyList<T> source,
            Func<T, TResult> selector)
        {
            var min = source.Min(selector);
            var max = source.Max(selector);
            return (min!, max!);
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
            Contract.Assert(source != null, nameof(source));
            using var it = source.GetEnumerator();
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

        public static IEnumerable<ValueTuple<T, T>> Pairwise<T>(this IEnumerable<T> source)
        {
            Contract.Assert(source != null, nameof(source));
            var isPair = false;
            var tempItem = default(T);
            foreach (var item in source)
            {
                if (isPair)
                {
                    yield return ValueTuple.Create(tempItem!, item);
                    isPair = false;
                }
                else
                {
                    tempItem = item;
                    isPair = true;
                }
            }
        }

        public static IEnumerable<ValueTuple<T, T, T>> Triplewise<T>(this IEnumerable<T> source)
        {
            Contract.Assert(source != null, nameof(source));
            var saved = 0;
            var temp1 = default(T);
            var temp2 = default(T);
            foreach (var item in source)
            {
                if (saved == 0)
                {
                    temp1 = item;
                    saved++;
                }
                else if (saved == 1)
                {
                    temp2 = item;
                    saved++;
                }
                else
                {
                    saved = 0;
                    yield return ValueTuple.Create(temp1!, temp2!, item);
                }
            }
        }

        public static IEnumerable<ImmutableList<T>> Chunks<T>(this IEnumerable<T> enumerable, EqualityComparer<T>? comparer = null)
        {
            Contract.Assert(enumerable != null, nameof(enumerable));
            comparer ??= EqualityComparer<T>.Default;

            var chunk = ImmutableList.CreateBuilder<T>();
            var reference = default(T);

            using var e = enumerable.GetEnumerator();
            while (e.MoveNext())
            {
                if (chunk.Count == 0)
                {
                    reference = e.Current;
                }
                else if (!comparer.Equals(e.Current, reference))
                {
                    yield return chunk.ToImmutable();
                    chunk = ImmutableList.CreateBuilder<T>();
                    reference = e.Current;
                }
                chunk.Add(e.Current);
            }
            yield return chunk.ToImmutable();
        }

        public static IEnumerable<ReadOnlyMemory<char>> Chunks(this string source, EqualityComparer<char>? comparer = null)
        {
            Contract.Assert(source != null, nameof(source));
            comparer ??= EqualityComparer<char>.Default;

            var startIdx = 0;
            var strMem = source.AsMemory();

            for (int i = 0; i < source.Length; i++)
            {
                if (!comparer.Equals(source[i], source[startIdx]))
                {
                    yield return strMem[startIdx..i];
                    startIdx = i;
                }
            }
            yield return strMem[startIdx..];
        }

        // https://stackoverflow.com/questions/419019/split-list-into-sublists-with-linq/20953521#20953521
        public static IEnumerable<IEnumerable<T>> Chunks<T>(this IEnumerable<T> enumerable,
            int chunkSize)
        {
            Contract.Assert(enumerable != null, nameof(enumerable));
            if (chunkSize < 1)
            {
                throw new ArgumentException("chunkSize must be positive");
            }

            using var e = enumerable.GetEnumerator();
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

        private static IEnumerable<T> GetChunk<T>(this IEnumerator<T> e,
            Func<bool> innerMoveNext)
        {
            do
            {
                yield return e.Current;
            } while (innerMoveNext());
        }

        public static IEnumerable<IList<Point>> PointsInBoundingRect(this IEnumerable<Point> points)
        {
            var list = new NonEmptyList<Point>(points);
            var (minx, maxx) = list.MinMax(p => p.X);
            var (miny, maxy) = list.MinMax(p => p.Y);

            for (int y = miny; y <= maxy; y++)
            {
                yield return Enumerable.Range(minx, maxx - minx + 1).Select(x => new Point(x, y)).ToList();
            }
        }

        public static IEnumerable<int> StartingIndex<T>(this IList<T> x, IList<T> y)
        {
            Contract.Assert(x != null, nameof(x));
            Contract.Assert(y != null, nameof(y));
            // https://stackoverflow.com/a/1780481
            IEnumerable<int> index = Enumerable.Range(0, x.Count - y.Count + 1);
            for (int i = 0; i < y.Count; i++)
            {
                index = index.Where(n => x[n + i]!.Equals(y[i])).ToArray();
            }
            return index;
        }

        public static IEnumerable<T> RepeatIndefinitely<T>(this IEnumerable<T> source)
        {
            var list = source.ToList();
            while (true)
            {
                foreach (var item in list)
                {
                    yield return item;
                }
            }
        }

        public static IEnumerable<T> Repeat<T>(this IEnumerable<T> source, int count = 2)
        {
            var list = source.ToList();
            return Enumerable.Repeat(list, count).SelectMany(l => l);
        }
        public static IEnumerable<int> CumulativeSum(this IEnumerable<int> sequence)
        {
            Contract.Assert(sequence != null, nameof(sequence));
            int sum = 0;
            foreach (var item in sequence)
            {
                sum += item;
                yield return sum;
            }
        }
        public static IEnumerable<(int x, int y, T value)> WithXY<T>(this IEnumerable<IEnumerable<T>> sequence)
            => sequence.SelectMany((line, y) => line.Select((item, x) => (x, y, item)));

        public static IEnumerable<TResult> Select2D<T, TResult>(this IEnumerable<IEnumerable<T>> sequence, Func<int, int, T, TResult> selector)
            => sequence.SelectMany((line, y) => line.Select((item, x) => selector(x, y, item)));

        public static ValueTuple<T, T> ToTuple2<T>(this IList<T> source) => (source[0], source[1]);
        public static ValueTuple<T, T, T> ToTuple3<T>(this IList<T> source) => (source[0], source[1], source[2]);
        public static ValueTuple<T, T, T, T> ToTuple4<T>(this IList<T> source) => (source[0], source[1], source[2], source[3]);


        public static IEnumerable<string[]> MatchRegexGroups(this IEnumerable<string> source, string pattern, int? count = null)
        {
            var regex = new Regex(pattern, RegexOptions.Singleline, TimeSpan.FromMilliseconds(100));
            foreach (var line in source)
            {
                ICollection<Group> matches = regex.Match(line).Groups;
                if (count != null)
                    Debug.Assert(matches.Count == count);
                yield return matches.Select(x => x.Value).ToArray();
            }
        }
    }
}
