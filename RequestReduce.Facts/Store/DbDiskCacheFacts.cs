using System;
using System.Web;
using Moq;
using RequestReduce.Configuration;
using RequestReduce.SqlServer;
using RequestReduce.Utilities;
using Xunit;
using System.Collections.Generic;

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
            public Dictionary<string, DateTime> FileList { get { return fileList; } }
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
            public void WillDeleteFilesOlderThanFiveMinutes()
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
                testable.Mock<IFileWrapper>().Verify(x => x.DeleteFile("url1"), Times.AtLeastOnce());
                testable.Mock<IFileWrapper>().Verify(x => x.DeleteFile("url3"), Times.AtLeastOnce());
            }

            [Fact]
            public void WillSwallowError()
            {
                var testable = new TestableDbDiskCache();
                testable.ClassUnderTest.FileList["url1"] = DateTime.Now.Subtract(new TimeSpan(0, 5, 1));
                testable.Mock<IFileWrapper>().Setup(x => x.DeleteFile(It.IsAny<string>())).Throws(new Exception());

                var ex = Record.Exception(() => testable.ClassUnderTest.PurgeOldFiles());

                Assert.Null(ex);
            }
        }

        public class Save
        {
            [Fact]
            public void WillAddUrlToList()
            {
                var testable = new TestableDbDiskCache();
                testable.Mock<IUriBuilder>().Setup(x => x.ParseSignature("key-sig-name.css")).Returns("sig");

                testable.ClassUnderTest.Save(new byte[] { 1 }, "key-sig-name.css", null);

                Assert.True(DateTime.Now.Subtract(testable.ClassUnderTest.FileList["key-sig-name.css"]).TotalSeconds < 1);
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

             [Fact]
             public void WillUpdateTimestampWhenTransmittingFile()
             {
                 var testable = new TestableDbDiskCache();
                 testable.ClassUnderTest.FileList["url"] = DateTime.Now.Subtract(new TimeSpan(0,3,0));
                 var response = new Mock<HttpResponseBase>();

                 testable.ClassUnderTest.SendContent("url", response.Object);

                 Assert.True(testable.ClassUnderTest.FileList["url"] > DateTime.Now.Subtract(new TimeSpan(0, 1, 0)));
             }
        }
    }
}
