using System;
using System.Collections.Concurrent;
using System.Web;
using Moq;
using RequestReduce.Configuration;
using RequestReduce.Store;
using RequestReduce.Utilities;
using Xunit;

namespace RequestReduce.Facts.Store
{
    public class DbDiskCacheFacts
    {
        class FakeDbDiskCache : DbDiskCache
        {
            public FakeDbDiskCache(IFileWrapper fileWrapper, IRRConfiguration configuration, IUriBuilder uriBuilder)
                : base(fileWrapper, configuration, uriBuilder)
            {
                timer.Dispose();
            }

            public void PurgeOldFiles()
            {
                PurgeOldFiles(null);
            }

            protected override string GetFileNameFromConfig(string url)
            {
                return url;
            }
            public ConcurrentDictionary<string, DateTime> FileList { get { return fileList; } }
        }

        class TestableDbDiskCache : Testable<FakeDbDiskCache>
        {
            public TestableDbDiskCache()
            {
                
            }
        }

        public class PurgeOldFiles
        {
            [Fact]
            public void WillDeeteFilesOlderThanFiveMinutes()
            {
                var testable = new TestableDbDiskCache();
                testable.ClassUnderTest.FileList["url1"] = DateTime.Now.Subtract(new TimeSpan(0, 5, 1));
                testable.ClassUnderTest.FileList["url2"] = DateTime.Now;
                testable.ClassUnderTest.FileList["url3"] = DateTime.Now.Subtract(new TimeSpan(0, 5, 1));
                testable.ClassUnderTest.FileList["url4"] = DateTime.Now;

                testable.ClassUnderTest.PurgeOldFiles();

                Assert.Equal(2, testable.ClassUnderTest.FileList.Count);
                Assert.True(testable.ClassUnderTest.FileList.ContainsKey("url2"));
                Assert.True(testable.ClassUnderTest.FileList.ContainsKey("url4"));
                testable.Mock<IFileWrapper>().Verify(x => x.DeleteFile("url1"), Times.Once());
                testable.Mock<IFileWrapper>().Verify(x => x.DeleteFile("url3"), Times.Once());
            }
        }

        public class Save
        {
            [Fact]
            public void WillAddUrlToList()
            {
                var testable = new TestableDbDiskCache();

                testable.ClassUnderTest.Save(new byte[]{1}, "url", null);

                Assert.True(DateTime.Now.Subtract(testable.ClassUnderTest.FileList["url"]).TotalSeconds < 1);
            }
        }

        public class SendContent
        {
             [Fact]
            public void WillNotTransmitFileIfNotInList()
             {
                 var testable = new TestableDbDiskCache();
                 var response = new Mock<HttpResponseBase>();

                 testable.ClassUnderTest.SendContent("url", response.Object);

                 response.Verify(x => x.TransmitFile("url"), Times.Never());
             }

             [Fact]
             public void WillTransmitFileIfInList()
             {
                 var testable = new TestableDbDiskCache();
                 testable.ClassUnderTest.FileList["url"] = DateTime.Now;
                 var response = new Mock<HttpResponseBase>();

                 testable.ClassUnderTest.SendContent("url", response.Object);

                 response.Verify(x => x.TransmitFile("url"), Times.Once());
             }

        }
    }
}
