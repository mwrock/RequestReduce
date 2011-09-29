using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using Moq;
using RequestReduce.Configuration;
using RequestReduce.ResourceTypes;
using RequestReduce.Utilities;
using Xunit;
using TimeoutException = Xunit.Sdk.TimeoutException;
using UriBuilder = RequestReduce.Utilities.UriBuilder;

namespace RequestReduce.Facts.Integration
{
    public class ModuleFacts
    {
        private readonly string rrFolder;
        private readonly UriBuilder uriBuilder;

        public ModuleFacts()
        {
            rrFolder = IntegrationFactHelper.ResetPhysicalContentDirectoryAndConfigureStore(Configuration.Store.LocalDiskStore);
            uriBuilder = new UriBuilder(new Mock<IRRConfiguration>().Object);
        }

        [OutputTraceOnFailFact]
        public void WillReduceToOneCssAndScript()
        {
            new WebClient().DownloadString("http://localhost:8877/Local.html");
            WaitToCreateResources();

            var response = new WebClient().DownloadString("http://localhost:8877/Local.html");

            Assert.Equal(1, new CssResource().ResourceRegex.Matches(response).Count);
            Assert.Equal(1, new JavaScriptResource().ResourceRegex.Matches(response).Count);
        }

        [OutputTraceOnFailFact]
        public void WillUseSameReductionAfterAppPoolRecycle()
        {
            var urlPattern = new Regex(@"(href|src)=""?(?<url>[^"" ]+)""?[^ />]+[ />]", RegexOptions.IgnoreCase);
            new WebClient().DownloadString("http://localhost:8877/Local.html");
            WaitToCreateResources();
            var response = new WebClient().DownloadString("http://localhost:8877/Local.html");
            var css = new CssResource().ResourceRegex.Match(response).ToString();
            var js = new JavaScriptResource().ResourceRegex.Match(response).ToString();
            var urls = new string[] {urlPattern.Match(css).Groups["url"].Value, urlPattern.Match(js).Groups["url"].Value};
            var files = new string [] {urls[0].Replace("/RRContent", rrFolder).Replace("/", "\\"),urls[1].Replace("/RRContent", rrFolder).Replace("/", "\\")};
            var createTime = new DateTime[] {new FileInfo(files[0]).LastWriteTime, new FileInfo(files[1]).LastWriteTime};

            IntegrationFactHelper.RecyclePool();
            new WebClient().DownloadString("http://localhost:8877/Local.html");
            WaitToCreateResources();

            Assert.Equal(createTime[0], new FileInfo(files[0]).LastWriteTime);
            Assert.Equal(createTime[1], new FileInfo(files[1]).LastWriteTime);
        }

        [OutputTraceOnFailFact]
        public void WillSetCacheHeadersOnCssContent()
        {
            var urlPattern = new Regex(@"(href|src)=""?(?<url>[^"" ]+)""?[^ />]+[ />]", RegexOptions.IgnoreCase);
            string url;
            using (var client = new WebClient())
            {
                client.DownloadString("http://localhost:8877/Local.html");
                WaitToCreateResources();
                var response = client.DownloadString("http://localhost:8877/Local.html");
                var css = new CssResource().ResourceRegex.Match(response).ToString();
                url = urlPattern.Match(css).Groups["url"].Value;
            }

            var req = HttpWebRequest.Create("http://localhost:8877" + url);
            var response2 = req.GetResponse();

            Assert.Equal("public", response2.Headers["Cache-Control"].ToLower());
            Assert.Equal("text/css", response2.ContentType);
            Assert.Equal(uriBuilder.ParseSignature(url), response2.Headers["ETag"]);
            response2.Close();
        }

        [OutputTraceOnFailFact]
        public void WillSetCacheHeadersOnJsContent()
        {
            var urlPattern = new Regex(@"(href|src)=""?(?<url>[^"" ]+)""?[^ />]+[ />]", RegexOptions.IgnoreCase);
            string url;
            using (var client = new WebClient())
            {
                client.DownloadString("http://localhost:8877/Local.html");
                WaitToCreateResources();
                var response = client.DownloadString("http://localhost:8877/Local.html");
                var js = new JavaScriptResource().ResourceRegex.Match(response).ToString();
                url = urlPattern.Match(js).Groups["url"].Value;
            }

            var req = HttpWebRequest.Create("http://localhost:8877" + url);
            var response2 = req.GetResponse();

            Assert.Equal("public", response2.Headers["Cache-Control"].ToLower());
            Assert.Equal("application/x-javascript", response2.ContentType);
            Assert.Equal(uriBuilder.ParseSignature(url), response2.Headers["ETag"]);
            response2.Close();
        }

        [OutputTraceOnFailFact]
        public void WillReReduceResourceAfterFileDeletion()
        {
            var cssPattern = new Regex(@"<link[^>]+type=""?text/css""?[^>]+>", RegexOptions.IgnoreCase);
            var urlPattern = new Regex(@"href=""?(?<url>[^"" ]+)""?[^ />]+[ />]", RegexOptions.IgnoreCase);
            new WebClient().DownloadString("http://localhost:8877/Local.html");
            WaitToCreateResources();
            var response = new WebClient().DownloadString("http://localhost:8877/Local.html");
            var css = cssPattern.Match(response).ToString();
            var url = urlPattern.Match(css).Groups["url"].Value;
            var file = url.Replace("/RRContent", rrFolder).Replace("/", "\\");
            var createTime = new FileInfo(file).LastWriteTime;

            File.Delete(file);
            while (File.Exists(file))
                Thread.Sleep(0);
            Thread.Sleep(100);
            new WebClient().DownloadString("http://localhost:8877/Local.html");
            WaitToCreateResources();
            new WebClient().DownloadString("http://localhost:8877/Local.html");

            Assert.True(createTime < new FileInfo(file).LastWriteTime);
        }

        [OutputTraceOnFailFact]
        public void WillFlushSingleReduction()
        {
            var cssPattern = new Regex(@"<link[^>]+type=""?text/css""?[^>]+>", RegexOptions.IgnoreCase);
            var urlPattern = new Regex(@"href=""?(?<url>[^"" ]+)""?[^ />]+[ />]", RegexOptions.IgnoreCase);
            new WebClient().DownloadString("http://localhost:8877/Local.html");
            WaitToCreateResources();
            var response = new WebClient().DownloadString("http://localhost:8877/Local.html");
            var css = cssPattern.Match(response).ToString();
            var url = urlPattern.Match(css).Groups["url"].Value;
            var key = uriBuilder.ParseKey(url).RemoveDashes();

            new WebClient().DownloadData("http://localhost:8877/RRContent/" + key + "/flush");
            var cssFilesAfterFlush = Directory.GetFiles(rrFolder, "*.css");
            response = new WebClient().DownloadString("http://localhost:8877/Local.html");
            css = cssPattern.Match(response).ToString();
            url = urlPattern.Match(css).Groups["url"].Value;
            var newKey = uriBuilder.ParseKey(url);
            WaitToCreateResources();
            var cssFilesAfterRefresh = Directory.GetFiles(rrFolder, "*.css");

            Assert.Equal(Guid.Empty, newKey);
            Assert.Equal(1, cssFilesAfterFlush.Length);
            Assert.True(cssFilesAfterFlush[0].Contains("-Expired-"));
            Assert.Equal(1, cssFilesAfterRefresh.Length);
            Assert.False(cssFilesAfterRefresh[0].Contains("-Expired-"));
        }

        private void WaitToCreateResources()
        {
            var watch = new Stopwatch();
            watch.Start();
            while (!Directory.Exists(rrFolder) && watch.ElapsedMilliseconds < 10000)
                Thread.Sleep(0);
            while (Directory.GetFiles(rrFolder, "*.css").Where(x => !x.Contains("-Expired")).Count() == 0 && watch.ElapsedMilliseconds < 10000)
                Thread.Sleep(0);
            while (Directory.GetFiles(rrFolder, "*.js").Where(x => !x.Contains("-Expired")).Count() == 0 && watch.ElapsedMilliseconds < 10000)
                Thread.Sleep(0);
            if (watch.ElapsedMilliseconds >= 10000)
                throw new TimeoutException(10000);
            Thread.Sleep(100);
        }
    }
}
