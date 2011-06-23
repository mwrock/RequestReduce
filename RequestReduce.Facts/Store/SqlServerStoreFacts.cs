using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
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
                testable.Mock<IFileRepository>().Setup(x => x.Save(It.IsAny<RequestReduceFile>())).Callback
                    <RequestReduceFile>(x => file = x);
                testable.Mock<IUriBuilder>().Setup(x => x.ParseFileName(url)).Returns("file.css");
                var key = Guid.NewGuid();
                testable.Mock<IUriBuilder>().Setup(x => x.ParseKey(url)).Returns(key);
                var id = Hasher.Hash(key + "file.css");

                testable.ClassUnderTest.Save(content, url, originalUrls);

                Assert.Equal(content, file.Content);
                Assert.Equal("file.css", file.FileName);
                Assert.Equal(key, file.Key);
                Assert.True(DateTime.Now.Subtract(file.LastUpdated).TotalMilliseconds < 1000);
                Assert.Equal(originalUrls, file.OriginalName);
                Assert.Equal(id, file.RequestReduceFileId);
            }

            [Fact]
            public void WillNotFireAddedEventIfFileIsPng()
            {
                var testable = new TestableSqlServerStore();
                var content = new byte[] { 1 };
                var expectedUrl = "url.png";
                var originalUrls = "originalUrls";
                var expectedKey = Guid.NewGuid();
                testable.Mock<IUriBuilder>().Setup(x => x.ParseKey(expectedUrl)).Returns(expectedKey);
                var key = new Guid();
                var url = string.Empty;
                testable.ClassUnderTest.CssAded += ((x, y) => { key = x; url = y; });

                testable.ClassUnderTest.Save(content, expectedUrl, originalUrls);

                Assert.Equal(string.Empty, url);
                Assert.Equal(Guid.Empty, key);
            }

            [Fact]
            public void WillFireAddedEventIfFileIsNotPng()
            {
                var testable = new TestableSqlServerStore();
                var content = new byte[] { 1 };
                var expectedUrl = "url.css";
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

        public class SendContent
        {
            [Fact]
            public void WillSendThroughFileCachedIfFileIsCached()
            {
                var testable = new TestableSqlServerStore();
                var response = new Mock<HttpResponseBase>();
                testable.Mock<IStore>().Setup(x => x.SendContent("url", response.Object)).Returns(true);

                var result = testable.ClassUnderTest.SendContent("url", response.Object);

                Assert.True(result);
                testable.Mock<IFileRepository>().Verify(x => x[It.IsAny<Guid>()], Times.Never());
            }

            [Fact]
            public void WillGoToDBAndWriteBytesIfFileIsNotCached()
            {
                var testable = new TestableSqlServerStore();
                var response = new Mock<HttpResponseBase>();
                testable.Mock<IStore>().Setup(x => x.SendContent("url", response.Object)).Returns(false);
                testable.Mock<IUriBuilder>().Setup(x => x.ParseFileName("url")).Returns("file.css");
                var key = Guid.NewGuid();
                testable.Mock<IUriBuilder>().Setup(x => x.ParseKey("url")).Returns(key);
                var id = Hasher.Hash(key + "file.css");
                var bytes = new byte[]{1};
                testable.Mock<IFileRepository>().Setup(x => x[id]).Returns(new RequestReduceFile(){Content = bytes});

                var result = testable.ClassUnderTest.SendContent("url", response.Object);

                Assert.True(result);
                response.Verify(x => x.BinaryWrite(bytes), Times.Once());
                testable.Mock<IStore>().Verify(x => x.Save(bytes, "url", null), Times.Once());
            }

            [Fact]
            public void WillReturnFalseAndFireDeletedEventIfFileIsNotInDepo()
            {
                var testable = new TestableSqlServerStore();
                var response = new Mock<HttpResponseBase>();
                testable.Mock<IStore>().Setup(x => x.SendContent("url", response.Object)).Returns(false);
                var key = Guid.NewGuid();
                testable.Mock<IUriBuilder>().Setup(x => x.ParseKey("url")).Returns(key);
                Guid triggeredKey = Guid.Empty;
                testable.ClassUnderTest.CssDeleted += (x => triggeredKey = x);

                var result = testable.ClassUnderTest.SendContent("url", response.Object);

                Assert.False(result);
                Assert.Equal(key, triggeredKey);
            }

            [Fact]
            public void WillRemoveFromReductionDictionaryIfExpired()
            {
                var testable = new TestableSqlServerStore();
                var response = new Mock<HttpResponseBase>();
                testable.Mock<IStore>().Setup(x => x.SendContent("url", response.Object)).Returns(false);
                testable.Mock<IUriBuilder>().Setup(x => x.ParseFileName("url")).Returns("file.css");
                var key = Guid.NewGuid();
                testable.Mock<IUriBuilder>().Setup(x => x.ParseKey("url")).Returns(key);
                var id = Hasher.Hash(key + "file.css");
                var bytes = new byte[] { 1 };
                testable.Mock<IFileRepository>().Setup(x => x[id]).Returns(new RequestReduceFile() { Content = bytes, IsExpired = true });
                var triggeredKey = Guid.Empty;
                testable.ClassUnderTest.CssDeleted += (x => triggeredKey = x);

                var result = testable.ClassUnderTest.SendContent("url", response.Object);

                Assert.True(result);
                response.Verify(x => x.BinaryWrite(bytes), Times.Once());
                Assert.Equal(triggeredKey, key);
            }
        }

        public class GetSavedUrls
        {
            [Fact]
            public void WillReturnKeysAndUrlsFromDB()
            {
                var testable = new TestableSqlServerStore();
                var guid1 = Guid.NewGuid();
                var guid2 = Guid.NewGuid();
                testable.Mock<IUriBuilder>().Setup(x => x.BuildCssUrl(guid1)).Returns("url1");
                testable.Mock<IUriBuilder>().Setup(x => x.BuildCssUrl(guid2)).Returns("url2");
                testable.Mock<IFileRepository>().Setup(x => x.GetKeys()).Returns(new List<Guid>()
                                                        {
                                                            guid1,
                                                            guid2
                                                        });

                var result = testable.ClassUnderTest.GetSavedUrls();

                Assert.Equal(2, result.Count);
                Assert.True(result[guid1] == "url1");
                Assert.True(result[guid2] == "url2");
            }
        }

        public class Flush
        {
            [Fact]
            public void WillExpireDbFileEntity()
            {
                var testable = new TestableSqlServerStore();
                var key = Guid.NewGuid();
                var file1 = new RequestReduceFile() {IsExpired = false, RequestReduceFileId = Guid.NewGuid()};
                var file2 = new RequestReduceFile() { IsExpired = false, RequestReduceFileId = Guid.NewGuid() };
                testable.Mock<IFileRepository>().Setup(x => x.GetFilesFromKey(key)).Returns(new RequestReduceFile[] {file1, file2});
                bool expire1 = false;
                bool expire2 = false;
                testable.Mock<IFileRepository>().Setup(x => x.Save(file1)).Callback<RequestReduceFile>(
                    y => expire1 = y.IsExpired);
                testable.Mock<IFileRepository>().Setup(x => x.Save(file2)).Callback<RequestReduceFile>(
                    y => expire2 = y.IsExpired);

                testable.ClassUnderTest.Flush(key);

                Assert.True(expire1);
                Assert.True(expire2);
            }

            [Fact]
            public void WillRemoveFromRepository()
            {
                var testable = new TestableSqlServerStore();
                var key = Guid.NewGuid();
                var triggeredKey = Guid.Empty;
                testable.ClassUnderTest.CssDeleted += (x => triggeredKey = x);

                testable.ClassUnderTest.Flush(key);

                Assert.Equal(key, triggeredKey);
            }

            [Fact]
            public void WillFlushAllKeysWhenPassedGuidIsEmpty()
            {
                var testable = new TestableSqlServerStore();
                var guid1 = Guid.NewGuid();
                var guid2 = Guid.NewGuid();
                testable.Mock<IUriBuilder>().Setup(x => x.BuildCssUrl(guid1)).Returns("url1");
                testable.Mock<IUriBuilder>().Setup(x => x.BuildCssUrl(guid2)).Returns("url2");
                testable.Mock<IFileRepository>().Setup(x => x.GetKeys()).Returns(new List<Guid>()
                                                        {
                                                            guid1,
                                                            guid2
                                                        });
                var file1 = new RequestReduceFile() { IsExpired = false, RequestReduceFileId = guid1 };
                var file2 = new RequestReduceFile() { IsExpired = false, RequestReduceFileId = guid2 };
                testable.Mock<IFileRepository>().Setup(x => x.GetFilesFromKey(guid1)).Returns(new RequestReduceFile[] { file1 });
                testable.Mock<IFileRepository>().Setup(x => x.GetFilesFromKey(guid2)).Returns(new RequestReduceFile[] { file2 });
                bool expire1 = false;
                bool expire2 = false;
                testable.Mock<IFileRepository>().Setup(x => x.Save(file1)).Callback<RequestReduceFile>(
                    y => expire1 = y.IsExpired);
                testable.Mock<IFileRepository>().Setup(x => x.Save(file2)).Callback<RequestReduceFile>(
                    y => expire2 = y.IsExpired);

                testable.ClassUnderTest.Flush(Guid.Empty);

                Assert.True(expire1);
                Assert.True(expire2);
            }
        }
    }
}
