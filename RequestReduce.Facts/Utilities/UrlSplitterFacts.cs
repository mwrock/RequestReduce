using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RequestReduce.Utilities;
using Xunit;

namespace RequestReduce.Facts.Utilities
{
    public class UrlSplitterFacts
    {
        [Fact]
        public void WillSplitUrlsIntoArray()
        {
            var testable = new UrlSplitter();

            var result = testable.Split("url1::url2::");

            Assert.True(result.Count() == 2);
            Assert.True(result.Contains("url1"));
            Assert.True(result.Contains("url2"));
        }
    }
}
