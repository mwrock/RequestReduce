using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Web;
using Moq;
using RequestReduce.Configuration;
using RequestReduce.Module;
using RequestReduce.Store;
using RequestReduce.Utilities;
using Xunit;
using UriBuilder = RequestReduce.Utilities.UriBuilder;
using RequestReduce.Reducer;
using RequestReduce.ResourceTypes;

namespace RequestReduce.Facts.Store
{
    public class LocalDiskStoreFacts
    {
        class FakeLocalDiskStore : LocalDiskStore
        {

            public FakeLocalDiskStore(IFileWrapper fileWrapper, IRRConfiguration configuration, IUriBuilder uriBuilder, IReductionRepository reductionRepository)
                : base(fileWrapper, configuration, uriBuilder, reductionRepository)
            {
            }

            protected override void SetupWatcher()
            {
                return;
            }

            public IReductionRepository ReductionRepository
            {
                set { reductionRepository = value; }
            }

        }

        class TestableLocalDiskStore : Testable<FakeLocalDiskStore>
        {
            public TestableLocalDiskStore()
            {
                
            }
        }

        class RealTestableLocalDiskStore : Testable<LocalDiskStore>
        {
            public RealTestableLocalDiskStore()
            {

            }
        }

        public class Save
        {
            [Fact]
            public void WillGetPhysicalPathFromUrl()
            {
                var testable = new TestableLocalDiskStore();
                var content = new byte[] {1};
                testable.Mock<IRRConfiguration>().Setup(x => x.SpriteVirtualPath).Returns("/url");
                testable.Mock<IRRConfiguration>().Setup(x => x.SpritePhysicalPath).Returns("c:\\web\\url");
                testable.Mock<IUriBuilder>().Setup(x => x.ParseSignature("/url/myid-style.cc")).Returns("style");

                testable.ClassUnderTest.Save(content, "/url/myid-style.cc", null);

                testable.Mock<IFileWrapper>().Verify(x => x.Save(content, "c:\\web\\url\\myid-style.cc"));
            }

            [Fact]
            public void WillGetPhysicalPathFromAbsoluteUrl()
            {
                var testable = new TestableLocalDiskStore();
                var content = new byte[] { 1 };
                testable.Mock<IRRConfiguration>().Setup(x => x.ContentHost).Returns("http://host");
                testable.Mock<IRRConfiguration>().Setup(x => x.SpriteVirtualPath).Returns("/url");
                testable.Mock<IRRConfiguration>().Setup(x => x.SpritePhysicalPath).Returns("c:\\web\\url");
                testable.Mock<IUriBuilder>().Setup(x => x.ParseSignature("http://host/url/myid-style.cc")).Returns("style");

                testable.ClassUnderTest.Save(content, "http://host/url/myid-style.cc", null);

                testable.Mock<IFileWrapper>().Verify(x => x.Save(content, "c:\\web\\url\\myid-style.cc"));
            }

            [Fact]
            public void WillRemoveExpiredFileIfExists()
            {
                var testable = new TestableLocalDiskStore();
                var content = new byte[] { 1 };
                testable.Mock<IRRConfiguration>().Setup(x => x.SpriteVirtualPath).Returns("/url");
                testable.Mock<IRRConfiguration>().Setup(x => x.SpritePhysicalPath).Returns("c:\\web\\url");
                testable.Mock<IFileWrapper>().Setup(x => x.FileExists("c:\\web\\url\\myid-Expired-style.cc")).Returns(
                    true);
                testable.Mock<IUriBuilder>().Setup(x => x.ParseSignature("/url/myid-style.cc")).Returns("style");

                testable.ClassUnderTest.Save(content, "/url/myid-style.cc", null);

                testable.Mock<IFileWrapper>().Verify(x => x.DeleteFile("c:\\web\\url\\myid-Expired-style.cc"));
            }

            [Fact]
            public void WillAddToReductionRepository()
            {
                var testable = new TestableLocalDiskStore();
                var content = new byte[] { 1 };
                testable.Mock<IRRConfiguration>().Setup(x => x.SpriteVirtualPath).Returns("/url");
                testable.Mock<IRRConfiguration>().Setup(x => x.SpritePhysicalPath).Returns("c:\\web\\url");
                testable.Mock<IUriBuilder>().Setup(x => x.ParseSignature("/url/myid-style.cc")).Returns("style");
                var expectedKey = Guid.NewGuid();
                testable.Mock<IUriBuilder>().Setup(x => x.ParseKey("/url/myid-style.cc")).Returns(expectedKey);

                testable.ClassUnderTest.Save(content, "/url/myid-style.cc", null);

                testable.Mock<IReductionRepository>().Verify(x => x.AddReduction(expectedKey, "/url/myid-style.cc"), Times.Once());
            }

            [Fact]
            public void WillNotAddToReductionRepositoryIfPng()
            {
                var testable = new TestableLocalDiskStore();
                var content = new byte[] { 1 };
                testable.Mock<IRRConfiguration>().Setup(x => x.SpriteVirtualPath).Returns("/url");
                testable.Mock<IRRConfiguration>().Setup(x => x.SpritePhysicalPath).Returns("c:\\web\\url");
                testable.Mock<IUriBuilder>().Setup(x => x.ParseSignature("/url/myid-style.png")).Returns("style");
                var expectedKey = Guid.NewGuid();
                testable.Mock<IUriBuilder>().Setup(x => x.ParseKey("/url/myid-style.png")).Returns(expectedKey);

                testable.ClassUnderTest.Save(content, "/url/myid-style.png", null);

                testable.Mock<IReductionRepository>().Verify(x => x.AddReduction(expectedKey, "/url/myid-style.png"), Times.Never());
            }

            [Fact]
            public void WillNotAddToReductionRepositoryIfItINull()
            {
                var testable = new TestableLocalDiskStore();
                var content = new byte[] { 1 };
                testable.Mock<IRRConfiguration>().Setup(x => x.SpriteVirtualPath).Returns("/url");
                testable.Mock<IRRConfiguration>().Setup(x => x.SpritePhysicalPath).Returns("c:\\web\\url");
                testable.Mock<IUriBuilder>().Setup(x => x.ParseSignature("/url/myid-style.cc")).Returns("style");
                var expectedKey = Guid.NewGuid();
                testable.Mock<IUriBuilder>().Setup(x => x.ParseKey("/url/myid-style.cc")).Returns(expectedKey);
                testable.ClassUnderTest.ReductionRepository = null;

                testable.ClassUnderTest.Save(content, "/url/myid-style.cc", null);

                testable.Mock<IReductionRepository>().Verify(x => x.AddReduction(expectedKey, "/url/myid-style.cc"), Times.Never());
            }
        }

        public class SendContent
        {
            [Fact]
            public void WillTransmitFileToResponseAndReturnTrue()
            {
                var testable = new TestableLocalDiskStore();
                var content = new byte[] { 1 };
                testable.Mock<IRRConfiguration>().Setup(x => x.SpriteVirtualPath).Returns("/url");
                testable.Mock<IRRConfiguration>().Setup(x => x.SpritePhysicalPath).Returns("c:\\web\\url");
                var response = new Mock<HttpResponseBase>();

                var result = testable.ClassUnderTest.SendContent("/url/myid-style.cc", response.Object);

                Assert.True(result);
                response.Verify(x => x.TransmitFile("c:\\web\\url\\myid-style.cc"), Times.Once());
            }

            [Fact]
            public void WillReturnFalseIfFileotFound()
            {
                var testable = new TestableLocalDiskStore();
                testable.Mock<IRRConfiguration>().Setup(x => x.SpriteVirtualPath).Returns("/url");
                testable.Mock<IRRConfiguration>().Setup(x => x.SpritePhysicalPath).Returns("c:\\web\\url");
                var response = new Mock<HttpResponseBase>();
                response.Setup(x => x.TransmitFile("c:\\web\\url\\myid-style.cc")).Throws(new FileNotFoundException());
                response.Setup(x => x.TransmitFile("c:\\web\\url\\myid-Expired-style.cc")).Throws(new FileNotFoundException());

                var result = testable.ClassUnderTest.SendContent("/url/myid-style.cc", response.Object);

                Assert.False(result);
            }

            [Fact]
            public void WillTransmitExpiredFileIfFileotFoundandExpired()
            {
                var testable = new TestableLocalDiskStore();
                testable.Mock<IRRConfiguration>().Setup(x => x.SpriteVirtualPath).Returns("/url");
                testable.Mock<IRRConfiguration>().Setup(x => x.SpritePhysicalPath).Returns("c:\\web\\url");
                var response = new Mock<HttpResponseBase>();
                response.Setup(x => x.TransmitFile("c:\\web\\url\\myid-style.cc")).Throws(new FileNotFoundException());

                var result = testable.ClassUnderTest.SendContent("/url/myid-style.cc", response.Object);

                Assert.True(result);
                response.Verify(x => x.TransmitFile("c:\\web\\url\\myid-Expired-style.cc"), Times.Once());
            }
        }

        public class GetSavedUrls
        {
            [Fact]
            public void WillCreateUrlsFromAllKeysInDirectory()
            {
                var testable = new TestableLocalDiskStore();
                testable.Mock<IRRConfiguration>().Setup(x => x.SpritePhysicalPath).Returns("dir");
                var guid1 = Guid.NewGuid();
                var guid2 = Guid.NewGuid();
                var sig1 = Guid.NewGuid().RemoveDashes();
                var sig2 = Guid.NewGuid().RemoveDashes();
                testable.Mock<IUriBuilder>().Setup(x => x.BuildResourceUrl(guid1, sig1, typeof(CssResource))).Returns("url1");
                testable.Mock<IUriBuilder>().Setup(x => x.BuildResourceUrl(guid2, sig2, typeof(CssResource))).Returns("url2");
                var files = new List<DatedFileEntry>
                                {
                                    new DatedFileEntry(string.Format("dir\\{0}-Expired-{1}-{2}", guid1.RemoveDashes(), sig1, new CssResource().FileName), DateTime.Now),
                                    new DatedFileEntry(string.Format("dir\\{0}-{1}-{2}", guid2.RemoveDashes(), sig2, new CssResource().FileName), DateTime.Now),
                                };
                testable.Mock<IFileWrapper>().Setup(x => x.GetDatedFiles("dir", "*RequestReduce*")).Returns(files);
                testable.Mock<IUriBuilder>().Setup(x => x.ParseKey(files[0].FileName.Replace("\\","/"))).Returns(guid1);
                testable.Mock<IUriBuilder>().Setup(x => x.ParseKey(files[1].FileName.Replace("\\", "/"))).Returns(guid2);
                testable.Mock<IUriBuilder>().Setup(x => x.ParseSignature(files[0].FileName.Replace("\\", "/"))).Returns(sig1);
                testable.Mock<IUriBuilder>().Setup(x => x.ParseSignature(files[1].FileName.Replace("\\", "/"))).Returns(sig2);

                var result = testable.ClassUnderTest.GetSavedUrls();

                Assert.Equal(1, result.Count);
                Assert.True(result[guid2] == "url2");
            }
			
			[Fact]
            public void WillPullMostRecentActiveUrlPerKey()
            {
                var testable = new TestableLocalDiskStore();
                testable.Mock<IRRConfiguration>().Setup(x => x.SpritePhysicalPath).Returns("dir");
                var guid1 = Guid.NewGuid();
                var guid2 = Guid.NewGuid();
                var sig1 = Guid.NewGuid().RemoveDashes();
                var sig2 = Guid.NewGuid().RemoveDashes();
                var sig3 = Guid.NewGuid().RemoveDashes();
                testable.Mock<IUriBuilder>().Setup(x => x.BuildResourceUrl(guid1, sig1, typeof(CssResource))).Returns("url1");
                testable.Mock<IUriBuilder>().Setup(x => x.BuildResourceUrl(guid2, sig2, typeof(CssResource))).Returns("url2");
                testable.Mock<IUriBuilder>().Setup(x => x.BuildResourceUrl(guid2, sig3, typeof(CssResource))).Returns("url3");
                var files = new List<DatedFileEntry>
                                {
                                    new DatedFileEntry(string.Format("dir\\{0}-Expired-{1}-{2}", guid1.RemoveDashes(), sig1, new CssResource().FileName), DateTime.Now),
                                    new DatedFileEntry(string.Format("dir\\{0}-{1}-{2}", guid2.RemoveDashes(), sig2, new CssResource().FileName), DateTime.Now),
					                new DatedFileEntry(string.Format("dir\\{0}-{1}-{2}", guid2.RemoveDashes(), sig3, new CssResource().FileName), DateTime.Now.Subtract(new TimeSpan(0,1,1)))
                                };
                testable.Mock<IFileWrapper>().Setup(x => x.GetDatedFiles("dir", "*RequestReduce*")).Returns(files);
                testable.Mock<IUriBuilder>().Setup(x => x.ParseKey(files[0].FileName.Replace("\\","/"))).Returns(guid1);
                testable.Mock<IUriBuilder>().Setup(x => x.ParseKey(files[1].FileName.Replace("\\", "/"))).Returns(guid2);
                testable.Mock<IUriBuilder>().Setup(x => x.ParseKey(files[2].FileName.Replace("\\", "/"))).Returns(guid2);
                testable.Mock<IUriBuilder>().Setup(x => x.ParseSignature(files[0].FileName.Replace("\\", "/"))).Returns(sig1);
                testable.Mock<IUriBuilder>().Setup(x => x.ParseSignature(files[1].FileName.Replace("\\", "/"))).Returns(sig2);
                testable.Mock<IUriBuilder>().Setup(x => x.ParseSignature(files[2].FileName.Replace("\\", "/"))).Returns(sig3);
				
                var result = testable.ClassUnderTest.GetSavedUrls();

                Assert.Equal(1, result.Count);
                Assert.True(result[guid2] == "url2");
            }

        }

        public class Flush
        {
            [Fact]
            public void WillExpireFile()
            {
                var testable = new TestableLocalDiskStore();
                var key = Guid.NewGuid();
                var urlBuilder = new UriBuilder(testable.Mock<IRRConfiguration>().Object);
                testable.Mock<IRRConfiguration>().Setup(x => x.SpriteVirtualPath).Returns("/RRContent");
                testable.Mock<IRRConfiguration>().Setup(x => x.SpritePhysicalPath).Returns("c:\\RRContent");
                testable.Inject<IUriBuilder>(urlBuilder);
                var file1 = urlBuilder.BuildResourceUrl<CssResource>(key, new byte[] { 1 }).Replace("/RRContent", "c:\\RRContent").Replace('/', '\\');
                var file2 = urlBuilder.BuildSpriteUrl(key, new byte[]{1}).Replace("/RRContent", "c:\\RRContent").Replace('/', '\\');
                testable.Mock<IFileWrapper>().Setup(x => x.GetFiles("c:\\RRContent")).Returns(new string[]
                                                                                                  {file1, file2});

                testable.ClassUnderTest.Flush(key);

                testable.Mock<IFileWrapper>().Verify(x => x.RenameFile(file1, file1.Replace(key.RemoveDashes(), key.RemoveDashes() + "-Expired")));
                testable.Mock<IFileWrapper>().Verify(x => x.RenameFile(file2, file2.Replace(key.RemoveDashes(), key.RemoveDashes() + "-Expired")));
            }

            [Fact]
            public void WillRemoveFromRepository()
            {
                var testable = new RealTestableLocalDiskStore();
                var key = Guid.NewGuid();

                testable.ClassUnderTest.Flush(key);

                testable.Mock<IReductionRepository>().Verify(x => x.RemoveReduction(key), Times.Once());
            }

            [Fact]
            public void WillFlushAllKeysWhenPassedGuidIsEmpty()
            {
                var testable = new TestableLocalDiskStore();
                var urlBuilder = new UriBuilder(testable.Mock<IRRConfiguration>().Object);
                testable.Inject(urlBuilder);
                var guid1 = Guid.NewGuid();
                var guid2 = Guid.NewGuid();
                testable.Mock<IRRConfiguration>().Setup(x => x.SpriteVirtualPath).Returns("/dir");
                testable.Mock<IRRConfiguration>().Setup(x => x.SpritePhysicalPath).Returns("c:\\web\\dir");
                testable.Inject<IUriBuilder>(urlBuilder);
                var file1 = new DatedFileEntry(urlBuilder.BuildResourceUrl<CssResource>(guid1, new byte[] { 1 }).Replace("/dir", "c:\\dir").Replace('/', '\\'), DateTime.Now);
                var file2 = new DatedFileEntry(urlBuilder.BuildResourceUrl<CssResource>(guid2, new byte[] { 1 }).Replace("/dir", "c:\\dir").Replace('/', '\\'), DateTime.Now);
                testable.Mock<IFileWrapper>().Setup(x => x.GetDatedFiles("c:\\web\\dir", "*RequestReduce*")).Returns(new List<DatedFileEntry> { file1, file2 });
                testable.Mock<IFileWrapper>().Setup(x => x.GetFiles("c:\\web\\dir")).Returns(new string[] { file1.FileName, file2.FileName });
				
                testable.ClassUnderTest.Flush(Guid.Empty);

                testable.Mock<IFileWrapper>().Verify(x => x.RenameFile(file1.FileName, file1.FileName.Replace(guid1.RemoveDashes(), guid1.RemoveDashes() + "-Expired")));
                testable.Mock<IFileWrapper>().Verify(x => x.RenameFile(file2.FileName, file2.FileName.Replace(guid2.RemoveDashes(), guid2.RemoveDashes() + "-Expired")));
            }
        }

    }
}
