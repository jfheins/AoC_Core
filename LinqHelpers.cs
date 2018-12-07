using System;
using System.Collections.Generic;
using System.Linq;

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
			return source.Select((value, index) => new {value, index})
				.Where(x => predicate(x.value))
				.Select(x => x.index);
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
        public static IEnumerable<Tuple<T, T>> PairwiseWithOverlap<T>(this IEnumerable<T> source)
		{
			using (var it = source.GetEnumerator())
			{
				if (!it.MoveNext())
					yield break;

				var previous = it.Current;

				while (it.MoveNext())
					yield return Tuple.Create(previous, previous = it.Current);
			}
		}

		public static IEnumerable<Tuple<T, T>> Pairwise<T>(this IEnumerable<T> source)
		{
			var isPair = false;
			var tempItem = default(T);
			foreach (var item in source)
				if (isPair)
				{
					yield return Tuple.Create(tempItem, item);
					isPair = false;
				}
				else
				{
					tempItem = item;
					isPair = true;
				}
		}

		// https://stackoverflow.com/questions/419019/split-list-into-sublists-with-linq/20953521#20953521
		public static IEnumerable<IEnumerable<T>> Chunks<T>(this IEnumerable<T> enumerable,
															int chunkSize)
		{
			if (chunkSize < 1)
				throw new ArgumentException("chunkSize must be positive");

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