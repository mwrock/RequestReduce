using System;
using Moq;
using RequestReduce.Store;
using RequestReduce.Utilities;
using Xunit;

namespace RequestReduce.Facts.Store
{
    public class SqlServerStoreFacts
    {
        class TestableSqlServerStore : Testable<SqlServerStore>
        {
            public TestableSqlServerStore()
            {
                
            }
        }

        public class Save
        {
            [Fact]
            public void WillSaveCssToRepository()
            {
                var testable = new TestableSqlServerStore();
                var content = new byte[] {1};
                var url = "url";
                var originalUrls = "originalUrls";
                RequestReduceFile file = null;
                testable.Mock<IRepository<RequestReduceFile>>().Setup(x => x.Save(It.IsAny<RequestReduceFile>())).Callback
                    <RequestReduceFile>(x => file = x);
                testable.Mock<IUriBuilder>().Setup(x => x.ParseFileName(url)).Returns("file.css");
                var key = Guid.NewGuid();
                testable.Mock<IUriBuilder>().Setup(x => x.ParseKey(url)).Returns(key);
                var id = Hasher.Hash(key + "file.css");

                testable.ClassUnderTest.Save(content, url, originalUrls);

                Assert.Equal(content, file.Content);
                Assert.Equal("file.css", file.FileName);
                Assert.Equal(key, file.Key);
                Assert.True(DateTime.Now.Subtract(file.LastAccessed).TotalMilliseconds < 1000);
                Assert.True(DateTime.Now.Subtract(file.LastUpdated).TotalMilliseconds < 1000);
                Assert.Equal(originalUrls, file.OriginalName);
                Assert.Equal(id, file.RequestReduceFileId);
            }

            [Fact]
            public void WillFireAddedEvent()
            {
                var testable = new TestableSqlServerStore();
                var content = new byte[] { 1 };
                var expectedUrl = "url";
                var originalUrls = "originalUrls";
                var expectedKey = Guid.NewGuid();
                testable.Mock<IUriBuilder>().Setup(x => x.ParseKey(expectedUrl)).Returns(expectedKey);
                var key = new Guid();
                var url = string.Empty;
                testable.ClassUnderTest.CssAded += ((x, y) => { key = x; url = y; });

                testable.ClassUnderTest.Save(content, expectedUrl, originalUrls);

                Assert.Equal(expectedUrl, url);
                Assert.Equal(expectedKey, key);
            }

            [Fact]
            public void WillSaveToFile()
            {
                var testable = new TestableSqlServerStore();
                var content = new byte[] { 1 };
                var expectedUrl = "url";
                var originalUrls = "originalUrls";

                testable.ClassUnderTest.Save(content, expectedUrl, originalUrls);

                testable.Mock<IStore>().Verify(x => x.Save(content, expectedUrl, originalUrls), Times.Once());
            }
        }
    }
}
