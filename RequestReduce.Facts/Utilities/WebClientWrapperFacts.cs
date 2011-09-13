using System;
using System.IO;
using System.Text;
using RequestReduce.Utilities;
using Xunit;

namespace RequestReduce.Facts.Utilities
{
    public class WebClientWrapperFacts
    {
        public class DownloadCssString
        {
            [Fact]
            public void WillNotIncludeUtf8PreambleInstring()
            {
                var wrapper = new WebClientWrapper();

                var result = wrapper.DownloadCssString("http://localhost:8877/styles/style1.css");

                Assert.False(result.StartsWith(Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble())));
            }

            [Fact]
            public void WillThrowErrorIfNotCss()
            {
                var wrapper = new WebClientWrapper();

                var ex = Assert.Throws<InvalidOperationException>(() => wrapper.DownloadCssString("http://localhost:8877/local.html"));

                Assert.NotNull(ex);
            }

        }

        public class DownloadJavaScriptString
        {
            [Fact]
            public void WillNotIncludeUtf8PreambleInstring()
            {
                var wrapper = new WebClientWrapper();

                var result = wrapper.DownloadJavaScriptString("http://localhost:8877/scripts/script1.js");

                Assert.False(result.StartsWith(Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble())));
            }

            [Fact]
            public void WillThrowErrorIfNotCss()
            {
                var wrapper = new WebClientWrapper();

                var ex = Assert.Throws<InvalidOperationException>(() => wrapper.DownloadJavaScriptString("http://localhost:8877/local.html"));

                Assert.NotNull(ex);
            }

        }
    }
}
