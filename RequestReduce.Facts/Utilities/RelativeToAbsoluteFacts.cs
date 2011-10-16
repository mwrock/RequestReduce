using System;
using RequestReduce.Utilities;
using Xunit;

namespace RequestReduce.Facts.Utilities
{
    public class RelativeToAbsoluteFacts
    {
        [Fact]
        public void WillMakeRelativeUrlAbsolute()
        {
            var result =
                RelativeToAbsoluteUtility.ToAbsolute("http://blogs.msdn.com/themes/blogs/MSDN2/css/MSDNblogs.css",
                                                     "../../../MSDN2/Images/MSDN/contentpane.png");

            Assert.Equal("http://blogs.msdn.com/themes/msdn2/Images/MSDN/contentpane.png", result, StringComparer.OrdinalIgnoreCase);
        }
    }
}
