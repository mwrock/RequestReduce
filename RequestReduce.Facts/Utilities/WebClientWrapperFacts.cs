using System;
using System.IO;
using System.Text;
using RequestReduce.Utilities;
using Xunit;

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
                var dir = string.Format("file://{0}/app.config", Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.FullName);

                var result = wrapper.DownloadString(dir);

                Assert.False(result.StartsWith(Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble())));
            }
        }
    }
}
