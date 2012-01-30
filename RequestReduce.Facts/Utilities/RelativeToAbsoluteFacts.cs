using System;
using Moq;
using RequestReduce.Api;
using RequestReduce.Configuration;
using RequestReduce.IOC;
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

        [Fact]
        public void WillMakeRelativeUrlAbsoluteAndAddContentHostIfItExists()
        {
            var config = new Mock<IRRConfiguration>();
            config.Setup(x => x.ContentHost).Returns("http://content");
            RRContainer.Current.Configure(x => x.For<IRRConfiguration>().Use(config.Object));

                var result =
                RelativeToAbsoluteUtility.ToAbsolute("http://blogs.msdn.com/themes/blogs/MSDN2/css/MSDNblogs.css",
                                                     "../../../MSDN2/Images/MSDN/contentpane.png");

            Assert.Equal("http://content/themes/msdn2/Images/MSDN/contentpane.png", result, StringComparer.OrdinalIgnoreCase);
            RRContainer.Current = null;
        }

        [Fact]
        public void WillNotUseContentHostIfUrlIsExternal()
        {
            var config = new Mock<IRRConfiguration>();
            config.Setup(x => x.ContentHost).Returns("http://content");
            RRContainer.Current.Configure(x => x.For<IRRConfiguration>().Use(config.Object));

            var result =
            RelativeToAbsoluteUtility.ToAbsolute("http://blogs.msdn.com/themes/blogs/MSDN2/css/MSDNblogs.css",
                                                 "http://gravatar.com/MSDN2/Images/MSDN/contentpane.png");

            Assert.Equal("http://gravatar.com/msdn2/Images/MSDN/contentpane.png", result, StringComparer.OrdinalIgnoreCase);
            RRContainer.Current = null;
        }

        [Fact]
        public void WillUseContentHostIfUrlIsLocal()
        {
            var config = new Mock<IRRConfiguration>();
            config.Setup(x => x.ContentHost).Returns("http://content");
            RRContainer.Current.Configure(x => x.For<IRRConfiguration>().Use(config.Object));

            var result =
            RelativeToAbsoluteUtility.ToAbsolute("http://blogs.msdn.com/themes/blogs/MSDN2/css/MSDNblogs.css",
                                                 "http://blogs.msdn.com/MSDN2/Images/MSDN/contentpane.png");

            Assert.Equal("http://content/msdn2/Images/MSDN/contentpane.png", result, StringComparer.OrdinalIgnoreCase);
            RRContainer.Current = null;
        }

        [Fact]
        public void WillForwardNewUrlToListener()
        {
            var config = new Mock<IRRConfiguration>();
            config.Setup(x => x.ContentHost).Returns("http://contenthost");
            RRContainer.Current.Configure(x => x.For<IRRConfiguration>().Use(config.Object));
            Registry.AbsoluteUrlTransformer  = (x, y) =>
                                                   {
                                                       var newUrlHost = new Uri(y).Host;
                                                       return y.Replace(newUrlHost, newUrlHost + "." + new Uri(x).Host);
                                                   };

            var result =
            RelativeToAbsoluteUtility.ToAbsolute("http://blogs.msdn.com/themes/blogs/MSDN2/css/MSDNblogs.css",
                                                 "../../../MSDN2/Images/MSDN/contentpane.png");

            Assert.Equal("http://contenthost.blogs.msdn.com/themes/msdn2/Images/MSDN/contentpane.png", result, StringComparer.OrdinalIgnoreCase);
            RRContainer.Current = null;
            Registry.AbsoluteUrlTransformer = null;
        }

        [Fact]
        public void WillForwardNewUrlToListenerIfNoContentHost()
        {
            Registry.AbsoluteUrlTransformer = (x, y) =>
            {
                var newUrlHost = new Uri(y).Host;
                return y.Replace(newUrlHost, "funny." + new Uri(x).Host);
            };

            var result =
            RelativeToAbsoluteUtility.ToAbsolute("http://blogs.msdn.com/themes/blogs/MSDN2/css/MSDNblogs.css",
                                                 "../../../MSDN2/Images/MSDN/contentpane.png");

            Assert.Equal("http://funny.blogs.msdn.com/themes/msdn2/Images/MSDN/contentpane.png", result, StringComparer.OrdinalIgnoreCase);
            Registry.AbsoluteUrlTransformer = null;
        }

    }
}
