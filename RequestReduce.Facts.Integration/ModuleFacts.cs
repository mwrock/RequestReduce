using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using RequestReduce.Configuration;
using Xunit;

namespace RequestReduce.Facts.Integration
{
    public class ModuleFacts
    {
        private readonly IRRConfiguration config = RRContainer.Current.GetInstance<IRRConfiguration>();

        [Fact]
        public void WillReduceToOneCss()
        {
            var cssPattern = new Regex(@"<link[^>]+type=""?text/css""?[^>]+>", RegexOptions.IgnoreCase);
            new WebClient().DownloadString("http://localhost:8888/Local.html");
            var watch = new Stopwatch();
            watch.Start();
            while(watch.ElapsedMilliseconds < 5000)
                Thread.Sleep(0);

            var response = new WebClient().DownloadString("http://localhost:8888/Local.html");

            Assert.Equal(1, cssPattern.Matches(response).Count);
        }
    }
}
