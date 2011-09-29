using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using Moq;
using RequestReduce.Configuration;
using RequestReduce.Store;
using RequestReduce.Utilities;
using Xunit;
using TimeoutException = Xunit.Sdk.TimeoutException;
using UriBuilder = RequestReduce.Utilities.UriBuilder;
using RequestReduce.ResourceTypes;

namespace RequestReduce.Facts.Integration
{
    public class SqlServerStoreFacts
    {
        private readonly IRRConfiguration config;
        private readonly IFileRepository repo;
        private readonly UriBuilder uriBuilder;
        private readonly string rrFolder;

        public SqlServerStoreFacts()
        {
            var dataDir = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName +
                          "\\RequestReduce.SampleWeb\\App_Data";
            if (!Directory.Exists(dataDir))
                Directory.CreateDirectory(dataDir);
            Database.DefaultConnectionFactory = new SqlCeConnectionFactory("System.Data.SqlServerCe.4.0");
            var mockConfig = new Mock<IRRConfiguration>();
            mockConfig.Setup(x => x.ConnectionStringName).Returns("data source=" + dataDir + "\\RequestReduce.sdf");
            config = mockConfig.Object;
            repo = new FileRepository(config);
            IntegrationFactHelper.RecyclePool();
            repo.Context.Database.Delete();
            repo.Context.Database.Create();
            rrFolder = IntegrationFactHelper.ResetPhysicalContentDirectoryAndConfigureStore(Configuration.Store.SqlServerStore);
            uriBuilder = new UriBuilder(config);
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
            var urls = new string[] { urlPattern.Match(css).Groups["url"].Value, urlPattern.Match(js).Groups["url"].Value };
            var files = new string[] { urls[0].Replace("/RRContent", rrFolder).Replace("/", "\\"), urls[1].Replace("/RRContent", rrFolder).Replace("/", "\\") };
            var createTime = new DateTime[] { new FileInfo(files[0]).LastWriteTime, new FileInfo(files[1]).LastWriteTime };

            IntegrationFactHelper.RecyclePool();
            new WebClient().DownloadString("http://localhost:8877/Local.html");
            WaitToCreateResources();

            Assert.Equal(createTime[0], new FileInfo(files[0]).LastWriteTime);
            Assert.Equal(createTime[1], new FileInfo(files[1]).LastWriteTime);
        }

        [OutputTraceOnFailFact]
        public void WillReReduceAfterFileIsRemovedFromDb()
        {
            var cssPattern = new Regex(@"<link[^>]+type=""?text/css""?[^>]+>", RegexOptions.IgnoreCase);
            var urlPattern = new Regex(@"href=""?(?<url>[^"" ]+)""?[^ />]+[ />]", RegexOptions.IgnoreCase);
            new WebClient().DownloadString("http://localhost:8877/Local.html");
            WaitToCreateResources();
            var response = new WebClient().DownloadString("http://localhost:8877/Local.html");
            var css = cssPattern.Match(response).ToString();
            var url = urlPattern.Match(css).Groups["url"].Value;
            var id = Guid.Parse(uriBuilder.ParseSignature(url));
            var createTime = repo[id].LastUpdated;

            repo.Context.Files.Remove(repo[id]);
            repo.Context.SaveChanges();
            IntegrationFactHelper.RecyclePool();
            new WebClient().DownloadString("http://localhost:8877/Local.html");
            WaitToCreateResources();
            new WebClient().DownloadString("http://localhost:8877/Local.html");

            Assert.True(createTime < repo[id].LastUpdated);
        }

        [OutputTraceOnFailFact]
        public void WillAccessContentFromFile()
        {
            var cssPattern = new Regex(@"<link[^>]+type=""?text/css""?[^>]+>", RegexOptions.IgnoreCase);
            var urlPattern = new Regex(@"href=""?(?<url>[^"" ]+)""?[^ />]+[ />]", RegexOptions.IgnoreCase);
            Guid id;
            string url;
            using (var client = new WebClient())
            {
                client.DownloadString("http://localhost:8877/Local.html");
                WaitToCreateResources();
                var response = client.DownloadString("http://localhost:8877/Local.html");
                var css = cssPattern.Match(response).ToString();
                url = urlPattern.Match(css).Groups["url"].Value;
                id = Guid.Parse(uriBuilder.ParseSignature(url));
            }
            repo.Detach(repo[id]);
            repo.Context.SaveChanges();

            var req = HttpWebRequest.Create("http://localhost:8877" + url);
            var response2 = req.GetResponse() as HttpWebResponse;

            Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
            response2.Close();
        }

        [OutputTraceOnFailFact]
        public void WillRecreateFileIfFileIsDeleted()
        {
            var cssPattern = new Regex(@"<link[^>]+type=""?text/css""?[^>]+>", RegexOptions.IgnoreCase);
            var urlPattern = new Regex(@"href=""?(?<url>[^"" ]+)""?[^ />]+[ />]", RegexOptions.IgnoreCase);
            string file;
            string url;
            using (var client = new WebClient())
            {
                client.DownloadString("http://localhost:8877/Local.html");
                WaitToCreateResources();
                var response = client.DownloadString("http://localhost:8877/Local.html");
                var css = cssPattern.Match(response).ToString();
                url = urlPattern.Match(css).Groups["url"].Value;
                file = url.Replace("/RRContent", rrFolder).Replace("/", "\\");
                File.Delete(file);
            }

            new WebClient().DownloadData("http://localhost:8877" + url);

            Assert.True(File.Exists(file));
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
            var oldKey = uriBuilder.ParseKey(url).RemoveDashes();
            var fileName = uriBuilder.ParseFileName(url);
            var firstCreated = File.GetLastWriteTime(rrFolder + "\\" + fileName);

            new WebClient().DownloadData("http://localhost:8877/RRContent/" + oldKey + "/flush");
            response = new WebClient().DownloadString("http://localhost:8877/Local.html");
            css = cssPattern.Match(response).ToString();
            url = urlPattern.Match(css).Groups["url"].Value;
            var newKey = uriBuilder.ParseKey(url);
            WaitToCreateResources();
            var secondCreated = File.GetLastWriteTime(rrFolder + "\\" + fileName);

            Assert.Equal(Guid.Empty, newKey);
            Assert.True(secondCreated > firstCreated);
        }

        [OutputTraceOnFailFact]
        public void WillUseSameReductionFromStoreIfAvailable()
        {
            var cssPattern = new Regex(@"<link[^>]+type=""?text/css""?[^>]+>", RegexOptions.IgnoreCase);
            new WebClient().DownloadString("http://localhost:8877/Local.html");
            WaitToCreateResources();
            new WebClient().DownloadData("http://localhost:8877/RRContent/flush");
            DateTime fileDate = DateTime.MinValue;
            var files = repo.AsQueryable().Where(x => x.FileName.Contains("RequestReduce"));
            foreach (var file in files)
            {
                file.IsExpired = false;
                fileDate = file.LastUpdated;
                repo.Save(file);
            }
            var response = new WebClient().DownloadString("http://localhost:8877/Local.html");
            var cssCount1 = cssPattern.Matches(response).Count;
            Thread.Sleep(400);

            response = new WebClient().DownloadString("http://localhost:8877/Local.html");

            var cssCount2 = cssPattern.Matches(response).Count;
            var file2 = repo.AsQueryable().First(x => x.FileName.Contains(".css"));
            Assert.Equal(2, cssCount1);
            Assert.Equal(1, cssCount2);
            Assert.Equal(fileDate, file2.LastUpdated);
        }

        private void WaitToCreateResources()
        {
            const int timeout = 20000;
            var watch = new Stopwatch();
            watch.Start();
            while (repo.AsQueryable().FirstOrDefault(x => x.FileName.Contains(".css") && !x.IsExpired) == null && watch.ElapsedMilliseconds < timeout)
                Thread.Sleep(0);
            while (repo.AsQueryable().FirstOrDefault(x => x.FileName.Contains(".js") && !x.IsExpired) == null && watch.ElapsedMilliseconds < timeout)
                Thread.Sleep(0);
            while (!Directory.Exists(rrFolder) && watch.ElapsedMilliseconds < timeout)
                Thread.Sleep(0);
            while (Directory.GetFiles(rrFolder, "*.css").Length == 0 && watch.ElapsedMilliseconds < timeout)
                Thread.Sleep(0);
            while (Directory.GetFiles(rrFolder, "*.js").Length == 0 && watch.ElapsedMilliseconds < timeout)
                Thread.Sleep(0);
            if (watch.ElapsedMilliseconds >= timeout)
                throw new TimeoutException(timeout);
            Thread.Sleep(100);
        }

    }
}
