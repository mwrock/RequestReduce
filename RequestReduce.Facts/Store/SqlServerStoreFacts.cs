using System;
using System.Collections.Generic;
using System.Web;
using Moq;
using RequestReduce.Module;
using RequestReduce.Store;
using RequestReduce.Utilities;
using Xunit;
using RequestReduce.Reducer;
using RequestReduce.ResourceTypes;
using RequestReduce.IOC;

namespace RequestReduce.Facts.Store
{
    public class SqlServerStoreFacts
    {
        class TestableSqlServerStore : Testable<SqlServerStore>
        {
            public TestableSqlServerStore()
            {
                Mock<IUriBuilder>().Setup(x => x.ParseSignature(It.IsAny<string>())).Returns(
                    Guid.NewGuid().RemoveDashes());
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
                var id = Guid.NewGuid();
                testable.Mock<IUriBuilder>().Setup(x => x.ParseKey(url)).Returns(key);
                testable.Mock<IUriBuilder>().Setup(x => x.ParseSignature(url)).Returns(() => id.RemoveDashes());

                testable.ClassUnderTest.Save(content, url, originalUrls);

                Assert.Equal(content, file.Content);
                Assert.Equal("file.css", file.FileName);
                Assert.Equal(key, file.Key);
                Assert.True(DateTime.Now.Subtract(file.LastUpdated).TotalMilliseconds < 1000);
                Assert.Equal(originalUrls, file.OriginalName);
                Assert.Equal(id, file.RequestReduceFileId);
            }

            [Fact]
            public void WillNotAddReductionIfFileIsPng()
            {
                var testable = new TestableSqlServerStore();
                var content = new byte[] { 1 };
                var expectedUrl = "url.png";
                var originalUrls = "originalUrls";
                var expectedKey = Guid.NewGuid();
                testable.Mock<IUriBuilder>().Setup(x => x.ParseKey(expectedUrl)).Returns(expectedKey);

                testable.ClassUnderTest.Save(content, expectedUrl, originalUrls);

                testable.Mock<IReductionRepository>().Verify(x => x.AddReduction(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never());
            }

            [Fact]
            public void WillAddReductionIfFileIsNotPng()
            {
                var testable = new TestableSqlServerStore();
                var content = new byte[] { 1 };
                var expectedUrl = "url.css";
                var originalUrls = "originalUrls";
                var expectedKey = Guid.NewGuid();
                testable.Mock<IUriBuilder>().Setup(x => x.ParseKey(expectedUrl)).Returns(expectedKey);

                testable.ClassUnderTest.Save(content, expectedUrl, originalUrls);

                testable.Mock<IReductionRepository>().Verify(x => x.AddReduction(expectedKey, expectedUrl), Times.Once());
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

            [Fact]
            public void WillUpdateExistingFileIfItExixtx()
            {
                var testable = new TestableSqlServerStore();
                var content = new byte[] { 1 };
                var url = "url";
                var originalUrls = "originalUrls";
                var key = Guid.NewGuid();
                var id = Guid.NewGuid();
                RequestReduceFile file = null;
                testable.Mock<IFileRepository>().Setup(x => x[id]).Returns(new RequestReduceFile() { IsExpired = true }).Verifiable();
                testable.Mock<IFileRepository>().Setup(x => x.Save(It.IsAny<RequestReduceFile>())).Callback
                    <RequestReduceFile>(x => file = x);
                testable.Mock<IUriBuilder>().Setup(x => x.ParseFileName(url)).Returns("file.css");
                testable.Mock<IUriBuilder>().Setup(x => x.ParseKey(url)).Returns(key);
                testable.Mock<IUriBuilder>().Setup(x => x.ParseSignature(url)).Returns(() => id.RemoveDashes());

                testable.ClassUnderTest.Save(content, url, originalUrls);

                testable.Mock<IFileRepository>().VerifyAll();
                Assert.Equal(content, file.Content);
                Assert.Equal("file.css", file.FileName);
                Assert.Equal(key, file.Key);
                Assert.True(DateTime.Now.Subtract(file.LastUpdated).TotalMilliseconds < 1000);
                Assert.Equal(originalUrls, file.OriginalName);
                Assert.Equal(id, file.RequestReduceFileId);
                Assert.False(file.IsExpired);
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
                var id = Guid.NewGuid();
                testable.Mock<IUriBuilder>().Setup(x => x.ParseSignature("url")).Returns(id.RemoveDashes());
                var key = Guid.NewGuid();
                testable.Mock<IUriBuilder>().Setup(x => x.ParseKey("url")).Returns(key);
                var bytes = new byte[]{1};
                testable.Mock<IFileRepository>().Setup(x => x[id]).Returns(new RequestReduceFile(){Content = bytes});

                var result = testable.ClassUnderTest.SendContent("url", response.Object);

                Assert.True(result);
                response.Verify(x => x.BinaryWrite(bytes), Times.Once());
                testable.Mock<IStore>().Verify(x => x.Save(bytes, "url", null), Times.Once());
            }

            [Fact]
            public void WillReturnFalseAndDeleteIfFileIsNotInRepo()
            {
                var testable = new TestableSqlServerStore();
                var response = new Mock<HttpResponseBase>();
                testable.Mock<IStore>().Setup(x => x.SendContent("url", response.Object)).Returns(false);
                var key = Guid.NewGuid();
                testable.Mock<IUriBuilder>().Setup(x => x.ParseKey("url")).Returns(key);

                var result = testable.ClassUnderTest.SendContent("url", response.Object);

                Assert.False(result);
                testable.Mock<IReductionRepository>().Verify(x => x.RemoveReduction(key), Times.Once());
            }

            [Fact]
            public void WillRemoveFromReductionDictionaryIfExpired()
            {
                var testable = new TestableSqlServerStore();
                var response = new Mock<HttpResponseBase>();
                testable.Mock<IStore>().Setup(x => x.SendContent("url", response.Object)).Returns(false);
                var id = Guid.NewGuid();
                testable.Mock<IUriBuilder>().Setup(x => x.ParseSignature("url")).Returns(id.RemoveDashes());
                var key = Guid.NewGuid();
                testable.Mock<IUriBuilder>().Setup(x => x.ParseKey("url")).Returns(key);
                var bytes = new byte[] { 1 };
                testable.Mock<IFileRepository>().Setup(x => x[id]).Returns(new RequestReduceFile() { Content = bytes, IsExpired = true });

                var result = testable.ClassUnderTest.SendContent("url", response.Object);

                Assert.True(result);
                response.Verify(x => x.BinaryWrite(bytes), Times.Once());
                testable.Mock<IReductionRepository>().Verify(x => x.RemoveReduction(key), Times.Once());
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
                var sig1 = Guid.NewGuid().RemoveDashes();
                var sig2 = Guid.NewGuid().RemoveDashes();
                var fileName1 = "1" + new CssResource().FileName;
                var fileName2 = "2" + new CssResource().FileName;
                testable.Mock<IUriBuilder>().Setup(x => x.BuildResourceUrl(guid1, sig1, typeof(CssResource))).Returns("url1");
                testable.Mock<IUriBuilder>().Setup(x => x.BuildResourceUrl(guid2, sig2, typeof(CssResource))).Returns("url2");
                testable.Mock<IUriBuilder>().Setup(x => x.ParseKey(fileName1)).Returns(guid1);
                testable.Mock<IUriBuilder>().Setup(x => x.ParseKey(fileName2)).Returns(guid2);
                testable.Mock<IUriBuilder>().Setup(x => x.ParseSignature(fileName1)).Returns(sig1);
                testable.Mock<IUriBuilder>().Setup(x => x.ParseSignature(fileName2)).Returns(sig2);
                testable.Mock<IFileRepository>().Setup(x => x.GetActiveFiles()).Returns(new List<string>()
                                                        {
                                                            fileName1,
                                                            fileName2
                                                        });

                var result = testable.ClassUnderTest.GetSavedUrls();

                Assert.Equal(2, result.Count);
                Assert.True(result[guid1] == "url1");
                Assert.True(result[guid2] == "url2");
                RRContainer.Current = null;
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

                testable.ClassUnderTest.Flush(key);

                testable.Mock<IReductionRepository>().Verify(x => x.RemoveReduction(key), Times.Once());
            }

            [Fact]
            public void WillFlushAllKeysWhenPassedGuidIsEmpty()
            {
                var testable = new TestableSqlServerStore();
                var guid1 = Guid.NewGuid();
                var guid2 = Guid.NewGuid();
                var sig1 = Guid.NewGuid().RemoveDashes();
                var sig2 = Guid.NewGuid().RemoveDashes();
                var fileName1 = "1" + new CssResource().FileName;
                var fileName2 = "2" + new CssResource().FileName;
                testable.Mock<IUriBuilder>().Setup(x => x.BuildResourceUrl(guid1, sig1, typeof(CssResource))).Returns("url1");
                testable.Mock<IUriBuilder>().Setup(x => x.BuildResourceUrl(guid2, sig2, typeof(CssResource))).Returns("url2");
                testable.Mock<IUriBuilder>().Setup(x => x.ParseKey(fileName1)).Returns(guid1);
                testable.Mock<IUriBuilder>().Setup(x => x.ParseKey(fileName2)).Returns(guid2);
                testable.Mock<IUriBuilder>().Setup(x => x.ParseSignature(fileName1)).Returns(sig1);
                testable.Mock<IUriBuilder>().Setup(x => x.ParseSignature(fileName2)).Returns(sig2);
                testable.Mock<IFileRepository>().Setup(x => x.GetActiveFiles()).Returns(new List<string>()
                                                        {
                                                            fileName1,
                                                            fileName2
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
                RRContainer.Current = null;
            }
        }

        public class GetUrlByKey
        {
            [Fact]
            public void WillGetUrlFromStoreIfItExists()
            {
                var testable = new TestableSqlServerStore();
                var guid1 = Guid.NewGuid();
                var sig1 = Guid.NewGuid().RemoveDashes();
                testable.Mock<IUriBuilder>().Setup(x => x.BuildResourceUrl(guid1, sig1, typeof(CssResource))).Returns("url1");
                testable.Mock<IUriBuilder>().Setup(x => x.ParseKey("file1")).Returns(guid1);
                testable.Mock<IUriBuilder>().Setup(x => x.ParseSignature("file1")).Returns(sig1);
                testable.Mock<IFileRepository>().Setup(x => x.GetActiveUrlByKey(guid1, typeof(CssResource))).Returns("file1");

                var result = testable.ClassUnderTest.GetUrlByKey(guid1, typeof(CssResource));

                Assert.Equal("url1", result);
            }

            [Fact]
            public void WillGetNullFromStoreIfItDoesNotExists()
            {
                var testable = new TestableSqlServerStore();
                testable.Mock<IUriBuilder>().Setup(x => x.BuildResourceUrl<CssResource>(It.IsAny<Guid>(), It.IsAny<string>())).Returns("url1");

                var result = testable.ClassUnderTest.GetUrlByKey(Guid.NewGuid(), typeof(CssResource));

                Assert.Null(result);
            }


        }
    }
}
