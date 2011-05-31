using System;
using System.Diagnostics;
using System.DirectoryServices;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using RequestReduce.Configuration;
using Xunit;

namespace RequestReduce.Facts.Integration
{
    public class ModuleFacts : IDisposable
    {
        private string rrFolder;

        public ModuleFacts()
        {
            rrFolder = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName + "\\RequestReduce.SampleWeb\\RRContent";
            if (Directory.Exists(rrFolder))
                Directory.Delete(rrFolder, true);
            RecyclePool();
        }

        [Fact]
        public void WillReduceToOneCss()
        {
            var cssPattern = new Regex(@"<link[^>]+type=""?text/css""?[^>]+>", RegexOptions.IgnoreCase);
            new WebClient().DownloadString("http://localhost:8888/Local.html");
            Thread.Sleep(5000);

            var response = new WebClient().DownloadString("http://localhost:8888/Local.html");

            Assert.Equal(1, cssPattern.Matches(response).Count);
        }

        [Fact]
        public void WillUseSameReductionAfterAppPoolRecycle()
        {
            var cssPattern = new Regex(@"<link[^>]+type=""?text/css""?[^>]+>", RegexOptions.IgnoreCase);
            var urlPattern = new Regex(@"href=""?(?<url>[^"" ]+)""?[^ />]+[ />]", RegexOptions.IgnoreCase);
            new WebClient().DownloadString("http://localhost:8888/Local.html");
            Thread.Sleep(5000);
            var response = new WebClient().DownloadString("http://localhost:8888/Local.html");
            var css = cssPattern.Match(response).ToString();
            var url = urlPattern.Match(css).Groups["url"].Value;
            var file = url.Replace("/RRContent", rrFolder).Replace("/", "\\");
            var createTime = new FileInfo(file).LastWriteTime;

            RecyclePool();
            new WebClient().DownloadString("http://localhost:8888/Local.html");
            Thread.Sleep(5000);

            Assert.Equal(createTime, new FileInfo(file).LastWriteTime);
        }

        private void RecyclePool()
        {
            var pool = new DirectoryEntry("IIS://localhost/W3SVC/AppPools/RequestReduce");
            pool.Invoke("Recycle", null);
            Thread.Sleep(1000);
        }

        public void Dispose()
        {
            if (Directory.Exists(rrFolder))
                Directory.Delete(rrFolder, true);
            RecyclePool();
        }
    }
}
