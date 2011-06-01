using System;
using RequestReduce.Configuration;
using RequestReduce.Store;
using RequestReduce.Utilities;
using Xunit;

namespace RequestReduce.Facts.Store
{
    public class LocalDiskStoreFacts
    {
        class TestableLocalDiskStore : Testable<LocalDiskStore>
        {
            public TestableLocalDiskStore()
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

                testable.ClassUnderTest.Save(content, "/url/myid/style.cc");

                testable.Mock<IFileWrapper>().Verify(x => x.Save(content, "c:\\web\\url\\myid\\style.cc"));
            }

            [Fact]
            public void WillGetPhysicalPathFromAbsoluteUrl()
            {
                var testable = new TestableLocalDiskStore();
                var content = new byte[] { 1 };
                testable.Mock<IRRConfiguration>().Setup(x => x.ContentHost).Returns("http://host");
                testable.Mock<IRRConfiguration>().Setup(x => x.SpriteVirtualPath).Returns("/url");
                testable.Mock<IRRConfiguration>().Setup(x => x.SpritePhysicalPath).Returns("c:\\web\\url");

                testable.ClassUnderTest.Save(content, "http://host/url/myid/style.cc");

                testable.Mock<IFileWrapper>().Verify(x => x.Save(content, "c:\\web\\url\\myid\\style.cc"));
            }

            [Fact]
            public void WillCreateKeyDirectoryIfMissing()
            {
                var testable = new TestableLocalDiskStore();
                var content = new byte[] { 1 };
                testable.Mock<IRRConfiguration>().Setup(x => x.SpriteVirtualPath).Returns("/url");
                testable.Mock<IRRConfiguration>().Setup(x => x.SpritePhysicalPath).Returns("c:\\web\\url");
                testable.Mock<IFileWrapper>().Setup(x => x.DirectoryExists("c:\\web\\url\\myid")).Returns(false);

                testable.ClassUnderTest.Save(content, "/url/myid/style.cc");

                testable.Mock<IFileWrapper>().Verify(x => x.CreateDirectory("c:\\web\\url\\myid"));
            }
        }

        public class OpenStream
        {
            [Fact]
            public void WillGetPhysicalPathFromUrl()
            {
                var testable = new TestableLocalDiskStore();
                testable.Mock<IRRConfiguration>().Setup(x => x.SpriteVirtualPath).Returns("/url");
                testable.Mock<IRRConfiguration>().Setup(x => x.SpritePhysicalPath).Returns("c:\\web\\url");

                testable.ClassUnderTest.OpenStream("/url/myid/style.cc");

                testable.Mock<IFileWrapper>().Verify(x => x.OpenStream("c:\\web\\url\\myid\\style.cc"));
            }

            [Fact]
            public void WillGetPhysicalPathFromAbsoluteUrl()
            {
                var testable = new TestableLocalDiskStore();
                testable.Mock<IRRConfiguration>().Setup(x => x.ContentHost).Returns("http://host");
                testable.Mock<IRRConfiguration>().Setup(x => x.SpriteVirtualPath).Returns("/url");
                testable.Mock<IRRConfiguration>().Setup(x => x.SpritePhysicalPath).Returns("c:\\web\\url");

                testable.ClassUnderTest.OpenStream("http://host/url/myid/style.cc");

                testable.Mock<IFileWrapper>().Verify(x => x.OpenStream("c:\\web\\url\\myid\\style.cc"));
            }

            [Fact]
            public void WillCreateKeyDirectoryIfMissing()
            {
                var testable = new TestableLocalDiskStore();
                testable.Mock<IRRConfiguration>().Setup(x => x.SpriteVirtualPath).Returns("/url");
                testable.Mock<IRRConfiguration>().Setup(x => x.SpritePhysicalPath).Returns("c:\\web\\url");
                testable.Mock<IFileWrapper>().Setup(x => x.DirectoryExists("c:\\web\\url\\myid")).Returns(false);

                testable.ClassUnderTest.OpenStream("/url/myid/style.cc");

                testable.Mock<IFileWrapper>().Verify(x => x.CreateDirectory("c:\\web\\url\\myid"));
            }
        }

        public class GetExistingUrls
        {
            [Fact]
            public void WillCreateUrlsFromAllKeysInDirectory()
            {
                var testable = new TestableLocalDiskStore();
                testable.Mock<IRRConfiguration>().Setup(x => x.SpritePhysicalPath).Returns("dir");
                var guid1 = Guid.NewGuid();
                var guid2 = Guid.NewGuid();
                testable.Mock<IUriBuilder>().Setup(x => x.BuildCssUrl(guid1)).Returns("url1");
                testable.Mock<IUriBuilder>().Setup(x => x.BuildCssUrl(guid2)).Returns("url2");
                testable.Mock<IFileWrapper>().Setup(x => x.GetDirectories("dir")).Returns(new string[]
                                                                                              {
                                                                                                  "dir\\" + guid1.ToString(),
                                                                                                  "dir\\" + guid2.ToString()
                                                                                              });

                var result = testable.ClassUnderTest.GetSavedUrls();

                Assert.Equal(2, result.Count);
                Assert.True(result[guid1] == "url1");
                Assert.True(result[guid2] == "url2");
            }
        }
    }
}
