using System;
using RequestReduce.Api;
using RequestReduce.Configuration;
using RequestReduce.Utilities;
using Xunit;

namespace RequestReduce.Facts.Utilities
{
    public class RelativeToAbsoluteFacts
    {
        private class TestableResponseFilter : Testable<RelativeToAbsoluteUtility>
        {
            public TestableResponseFilter()
            {
                Mock<IRRConfiguration>().Setup(x => x.ContentHost).Returns("http://content");
            }
        }

        [Fact]
        public void WillMakeRelativeUrlAbsolute()
        {
            var testable = new TestableResponseFilter();
            testable.Mock<IRRConfiguration>().Setup(x => x.ContentHost).Returns(string.Empty);

            var result =
                testable.ClassUnderTest.ToAbsolute("http://blogs.msdn.com/themes/blogs/MSDN2/css/MSDNblogs.css",
                                                     "../../../MSDN2/Images/MSDN/contentpane.png");

            Assert.Equal("http://blogs.msdn.com/themes/msdn2/Images/MSDN/contentpane.png", result, StringComparer.OrdinalIgnoreCase);
        }

        [Fact]
        public void WillMakeRelativeUrlAbsoluteAndAddContentHostIfItExists()
        {
            var testable = new TestableResponseFilter();

                var result =
                testable.ClassUnderTest.ToAbsolute("http://blogs.msdn.com/themes/blogs/MSDN2/css/MSDNblogs.css",
                                                     "../../../MSDN2/Images/MSDN/contentpane.png");

            Assert.Equal("http://content/themes/msdn2/Images/MSDN/contentpane.png", result, StringComparer.OrdinalIgnoreCase);
        }

        [Fact]
        public void WillNotUseContentHostIfUrlIsExternal()
        {
            var testable = new TestableResponseFilter();

            var result =
            testable.ClassUnderTest.ToAbsolute("http://blogs.msdn.com/themes/blogs/MSDN2/css/MSDNblogs.css",
                                                 "http://gravatar.com/MSDN2/Images/MSDN/contentpane.png");

            Assert.Equal("http://gravatar.com/msdn2/Images/MSDN/contentpane.png", result, StringComparer.OrdinalIgnoreCase);
        }

        [Fact]
        public void WillUseContentHostIfUrlIsLocal()
        {
            var testable = new TestableResponseFilter();

            var result =
            testable.ClassUnderTest.ToAbsolute("http://blogs.msdn.com/themes/blogs/MSDN2/css/MSDNblogs.css",
                                                 "http://blogs.msdn.com/MSDN2/Images/MSDN/contentpane.png");

            Assert.Equal("http://content/msdn2/Images/MSDN/contentpane.png", result, StringComparer.OrdinalIgnoreCase);
        }

        [Fact]
        public void WillForwardNewUrlToListener_obsolete()
        {
            var testable = new TestableResponseFilter();
#pragma warning disable 618
            Registry.AbsoluteUrlTransformer  = (x, y) =>
#pragma warning restore 618
                                                   {
                                                       var newUrlHost = new Uri(y).Host;
                                                       return y.Replace("http://" + newUrlHost, "http://" + newUrlHost + "." + new Uri(x).Host);
                                                   };

            var result =
            testable.ClassUnderTest.ToAbsolute("http://blogs.msdn.com/themes/blogs/MSDN2/css/MSDNblogs.css",
                                                 "../../../MSDN2/Images/MSDN/contentpane.png");

            Assert.Equal("http://content.blogs.msdn.com/themes/msdn2/Images/MSDN/contentpane.png", result, StringComparer.OrdinalIgnoreCase);
#pragma warning disable 618
            Registry.AbsoluteUrlTransformer = null;
#pragma warning restore 618
        }

        [Fact]
        public void WillForwardNewUrlToListenerIfNoContentHost_obsolete()
        {
            var testable = new TestableResponseFilter();
            testable.Mock<IRRConfiguration>().Setup(x => x.ContentHost).Returns(string.Empty);
#pragma warning disable 618
            Registry.AbsoluteUrlTransformer = (x, y) =>
#pragma warning restore 618
            {
                var newUrlHost = new Uri(y).Host;
                return y.Replace(newUrlHost, "funny." + new Uri(x).Host);
            };

            var result =
            testable.ClassUnderTest.ToAbsolute("http://blogs.msdn.com/themes/blogs/MSDN2/css/MSDNblogs.css",
                                                 "../../../MSDN2/Images/MSDN/contentpane.png");

            Assert.Equal("http://funny.blogs.msdn.com/themes/msdn2/Images/MSDN/contentpane.png", result, StringComparer.OrdinalIgnoreCase);
#pragma warning disable 618
            Registry.AbsoluteUrlTransformer = null;
#pragma warning restore 618
        }

        [Fact]
        public void WillForwardNewUrlToListener()
        {
            var testable = new TestableResponseFilter();
            Registry.UrlTransformer = (c, x, y) =>
            {
                var newUrlHost = new Uri(y).Host;
                return y.Replace("http://" + newUrlHost, "http://" + newUrlHost + "." + new Uri(x).Host);
            };

            var result =
            testable.ClassUnderTest.ToAbsolute("http://blogs.msdn.com/themes/blogs/MSDN2/css/MSDNblogs.css",
                                                 "../../../MSDN2/Images/MSDN/contentpane.png");

            Assert.Equal("http://content.blogs.msdn.com/themes/msdn2/Images/MSDN/contentpane.png", result, StringComparer.OrdinalIgnoreCase);
            Registry.UrlTransformer = null;
        }

        [Fact]
        public void WillForwardNewUrlToListenerIfNoContentHost()
        {
            var testable = new TestableResponseFilter();
            testable.Mock<IRRConfiguration>().Setup(x => x.ContentHost).Returns(string.Empty);
            Registry.UrlTransformer = (c, x, y) =>
            {
                var newUrlHost = new Uri(y).Host;
                return y.Replace(newUrlHost, "funny." + new Uri(x).Host);
            };

            var result =
            testable.ClassUnderTest.ToAbsolute("http://blogs.msdn.com/themes/blogs/MSDN2/css/MSDNblogs.css",
                                                 "../../../MSDN2/Images/MSDN/contentpane.png");

            Assert.Equal("http://funny.blogs.msdn.com/themes/msdn2/Images/MSDN/contentpane.png", result, StringComparer.OrdinalIgnoreCase);
            Registry.UrlTransformer = null;
        }

        [Fact]
        public void willMakeAbsoluteWithoutContentHostWhenPassingFalseForUseContentHost()
        {
            var testable = new TestableResponseFilter();

            var result =
            testable.ClassUnderTest.ToAbsolute("http://blogs.msdn.com/themes/blogs/MSDN2/css/MSDNblogs.css",
                                                 "/MSDN2/Images/MSDN/contentpane.png", false);

            Assert.Equal("http://blogs.msdn.com/msdn2/Images/MSDN/contentpane.png", result, StringComparer.OrdinalIgnoreCase);
        }
    }
}
