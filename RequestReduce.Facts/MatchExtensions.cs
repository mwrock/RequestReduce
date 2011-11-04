using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Moq;

namespace RequestReduce.Facts
{
    public static class MatchExtensions
    {
        public static T[] MatchEnumerable<T>(this T[] enumerable)
        {
            return Match.Create<T[]>(x => (enumerable.SequenceEqual(x)));
        }

        public static IEnumerable<T> MatchEnumerable<T>(this IEnumerable<T> enumerable)
        {
            return Match.Create<IEnumerable<T>>(x => (enumerable.SequenceEqual(x)));
        }
    }
}
