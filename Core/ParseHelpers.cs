using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Core
{
    public static class ParseHelpers
    {

        public static int[] ParseInts(this string str, int? count = null)
        {
            var regex = new Regex(@"([-+]?[0-9]+)");
            var matches = regex.Matches(str);
            if (count != null)
                Debug.Assert(matches.Count == count);

            return matches.Select(match => int.Parse(match.Value)).ToArray();
        }
        public static int[] ParseNNInts(this string str, int? count = null)
        {
            var regex = new Regex(@"([0-9]+)");
            var matches = regex.Matches(str);
            if (count != null)
                Debug.Assert(matches.Count == count);

            return matches.Select(match => int.Parse(match.Value)).ToArray();
        }

        public static long[] ParseLongs(this string str, int? count = null)
        {
            var regex = new Regex(@"([-+]?[0-9]+)");
            var matches = regex.Matches(str);
            if (count != null)
                Debug.Assert(matches.Count == count);

            return matches.Select(match => long.Parse(match.Value)).ToArray();
        }

        public static IEnumerable<T> MatchRegexGroup<T>(
            this IEnumerable<string> source,
            string pattern,
            int? count = null)
        {
            var regex = new Regex(pattern, RegexOptions.Singleline, TimeSpan.FromMilliseconds(100));
            static T ResultFactory(IList<Group> groups) => (T)Convert.ChangeType(groups[1].Value, typeof(T));
            return RegexIterator(source, regex, count, ResultFactory);
        }

        public static IEnumerable<ValueTuple<T1, T2>> MatchRegexGroups2<T1, T2>(
            this IEnumerable<string> source,
            string pattern,
            int? count = null)
        {
            var regex = new Regex(pattern, RegexOptions.Singleline, TimeSpan.FromMilliseconds(100));

            static (T1, T2) ResultFactory(IList<Group> groups)
            {
                T Parser<T>(int idx) => (T)Convert.ChangeType(groups[idx].Value, typeof(T));
                return (Parser<T1>(1), Parser<T2>(2));
            }

            return RegexIterator(source, regex, count, ResultFactory);
        }

        public static IEnumerable<ValueTuple<T1, T2, T3>> MatchRegexGroups3<T1, T2, T3>(
            this IEnumerable<string> source,
            string pattern,
            int? count = null)
        {
            var regex = new Regex(pattern, RegexOptions.Singleline, TimeSpan.FromMilliseconds(100));

            static (T1, T2, T3) ResultFactory(IList<Group> groups)
            {
                T Parser<T>(int idx) => (T)Convert.ChangeType(groups[idx].Value, typeof(T));
                return (Parser<T1>(1), Parser<T2>(2), Parser<T3>(3));
            }

            return RegexIterator(source, regex, count, ResultFactory);
        }

        public static IEnumerable<ValueTuple<T1, T2, T3, T4>> MatchRegexGroups4<T1, T2, T3, T4>(
            this IEnumerable<string> source,
            string pattern,
            int? count = null)
        {
            var regex = new Regex(pattern, RegexOptions.Singleline, TimeSpan.FromMilliseconds(100));

            static (T1, T2, T3, T4) ResultFactory(IList<Group> groups)
            {
                T Parser<T>(int idx) => (T)Convert.ChangeType(groups[idx].Value, typeof(T));
                return (Parser<T1>(1), Parser<T2>(2), Parser<T3>(3), Parser<T4>(4));
            }

            return RegexIterator(source, regex, count, ResultFactory);
        }

        private static IEnumerable<T> RegexIterator<T>(IEnumerable<string> source, Regex regex, int? count, Func<IList<Group>, T> resultFactory)
        {
            foreach (var line in source)
            {
                IList<Group> matches = regex.Match(line).Groups;
                if (count.HasValue)
                    Debug.Assert(matches.Count == count.Value);
                yield return resultFactory(matches);
            }
        }


    }
}
