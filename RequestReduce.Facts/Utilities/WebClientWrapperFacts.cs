using System;
using System.IO;
using System.Text;
using RequestReduce.Utilities;
using Xunit;
using RequestReduce.ResourceTypes;

namespace RequestReduce.Facts.Utilities
{
    public class WebClientWrapperFacts
    {
        public class DownloadString
        {
            [Fact]
            public void WillNotIncludeUtf8PreambleInstring()
            {
                var wrapper = new WebClientWrapper();

                var result = wrapper.DownloadString<CssResource>("http://localhost:8877/styles/style1.css");

                Assert.False(result.StartsWith(Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble())));
            }

            [Fact]
            public void WillThrowErrorIfNotCss()
            {
                var wrapper = new WebClientWrapper();

                var ex = Assert.Throws<InvalidOperationException>(() => wrapper.DownloadString<CssResource>("http://localhost:8877/local.html"));

                Assert.NotNull(ex);
            }

            [Fact]
            public void WillThrowErrorIfNotJavaScript()
            {
                var wrapper = new WebClientWrapper();

                var ex = Assert.Throws<InvalidOperationException>(() => wrapper.DownloadString<JavaScriptResource>("http://localhost:8877/local.html"));

                Assert.NotNull(ex);
            }

        }
    }
}
