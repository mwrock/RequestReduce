using System.Collections.Generic;
using RequestReduce.Reducer;
using Xunit;

namespace RequestReduce.Facts.Reducer
{
    public class SelectorComparerFacts
    {
        [Fact]
        public void WillOrderClassWithHighestSpecificityHighest()
        {
            var set = new SortedSet<BackgroundImageClass>(new SelectorComparer());
            set.Add(new BackgroundImageClass("h1{{color:blue}}", 0));
            set.Add(new BackgroundImageClass("h1#myid{{color:blue}}", 0));
            BackgroundImageClass result = null;

            foreach (var backgroundImageClass in set)
            {
                result = backgroundImageClass;
                break;
            }

            Assert.Equal("h1#myid", result.Selector);
        }

        [Fact]
        public void WillOrderLastClassWithSameScoreFirst()
        {
            var set = new SortedSet<BackgroundImageClass>(new SelectorComparer());
            set.Add(new BackgroundImageClass("h1{{width:5}}", 1));
            set.Add(new BackgroundImageClass("h1{{width:10}}", 2));
            BackgroundImageClass result = null;

            foreach (var backgroundImageClass in set)
            {
                result = backgroundImageClass;
                break;
            }

            Assert.Equal(10, result.Width);
        }

    }
}
