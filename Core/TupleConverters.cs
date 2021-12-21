using System;
using System.Collections.Generic;
using System.Linq;

namespace Core
{
    public static class TupleHelpers
    {
        public static int Diff(this ValueTuple<int, int> pair) => pair.Item2 - pair.Item1;

        public static long Diff(this ValueTuple<long, long> pair) => pair.Item2 - pair.Item1;

        public static double Diff(this ValueTuple<double, double> pair) => pair.Item2 - pair.Item1;

        public static IEnumerable<int> Diff(this IEnumerable<int> source)
            => source.PairwiseWithOverlap().Select(x => x.Diff());
        public static IEnumerable<long> Diff(this IEnumerable<long> source)
            => source.PairwiseWithOverlap().Select(x => x.Diff());
        public static IEnumerable<double> Diff(this IEnumerable<double> source)
            => source.PairwiseWithOverlap().Select(x => x.Diff());

        public static ValueTuple<T1, T2> ToTuple2<T1, T2>(this IEnumerable<string> source)
        {
            var list = source as IList<string> ?? source.ToList();
            return (Parser<T1>(0), Parser<T2>(1));
            T Parser<T>(int idx) => (T)Convert.ChangeType(list[idx], typeof(T));
        }
        public static ValueTuple<T1, T2, T3> ToTuple3<T1, T2, T3>(this IEnumerable<string> source)
        {
            var list = source as IList<string> ?? source.ToList();
            return (Parser<T1>(0), Parser<T2>(1), Parser<T3>(2));
            T Parser<T>(int idx) => (T)Convert.ChangeType(list[idx], typeof(T));
        }
        public static ValueTuple<T1, T2, T3, T4> ToTuple4<T1, T2, T3, T4>(this IEnumerable<string> source)
        {
            var list = source as IList<string> ?? source.ToList();
            return (Parser<T1>(0), Parser<T2>(1), Parser<T3>(2), Parser<T4>(3));
            T Parser<T>(int idx) => (T)Convert.ChangeType(list[idx], typeof(T));
        }

        public static ValueTuple<T, T> ToTuple2<T>(this IList<T> source) => (source[0], source[1]);
        public static ValueTuple<T, T, T> ToTuple3<T>(this IList<T> source) => (source[0], source[1], source[2]);
        public static ValueTuple<T, T, T, T> ToTuple4<T>(this IList<T> source) => (source[0], source[1], source[2], source[3]);

        public static ValueTuple<T, T> ToTuple2<T>(this IEnumerable<T> source) => source.ToList().ToTuple2();
        public static ValueTuple<T, T, T> ToTuple3<T>(this IEnumerable<T> source) => source.ToList().ToTuple3();
        public static ValueTuple<T, T, T, T> ToTuple4<T>(this IEnumerable<T> source) => source.ToList().ToTuple4();
    }
}
