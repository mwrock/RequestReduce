using System;
using System.IO;
using System.Web;
using Moq;
using RequestReduce.Configuration;
using RequestReduce.Store;
using RequestReduce.Utilities;
using Xunit;

namespace RequestReduce.Facts.Store
{
    public class LocalDiskStoreFacts
    {
        class FakeLocalDiskStore : LocalDiskStore
        {
            public FakeLocalDiskStore(IFileWrapper fileWrapper, IRRConfiguration configuration, IUriBuilder uriBuilder)
                : base(fileWrapper, configuration, uriBuilder)
            {
            }
            public override event DeleeCsAction CssDeleted;
            public override event AddCssAction CssAded;

            protected override void SetupWatcher()
            {
                return;
            }

            public void TriggerChange(string change, Guid key)
            {
                if (change == "delete")
                    CssDeleted(key);
                if (change == "add")
                    CssAded(key, "url");
            }
        }

        class TestableLocalDiskStore : Testable<FakeLocalDiskStore>
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

                testable.ClassUnderTest.Save(content, "/url/myid/style.cc", null);

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

                testable.ClassUnderTest.Save(content, "http://host/url/myid/style.cc", null);

                testable.Mock<IFileWrapper>().Verify(x => x.Save(content, "c:\\web\\url\\myid\\style.cc"));
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

                var result = testable.ClassUnderTest.SendContent("/url/myid/style.cc", response.Object);

                Assert.True(result);
                response.Verify(x => x.TransmitFile("c:\\web\\url\\myid\\style.cc"), Times.Once());
            }

            [Fact]
            public void WillReturnFalseIfFileotFound()
            {
                var testable = new TestableLocalDiskStore();
                var content = new byte[] { 1 };
                testable.Mock<IRRConfiguration>().Setup(x => x.SpriteVirtualPath).Returns("/url");
                testable.Mock<IRRConfiguration>().Setup(x => x.SpritePhysicalPath).Returns("c:\\web\\url");
                var response = new Mock<HttpResponseBase>();
                response.Setup(x => x.TransmitFile("c:\\web\\url\\myid\\style.cc")).Throws(new FileNotFoundException());

                var result = testable.ClassUnderTest.SendContent("/url/myid/style.cc", response.Object);

                Assert.False(result);
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
                var files = new string[]
                                {
                                    "dir\\" + guid1 + "-file.css",
                                    "dir\\" + guid2 + "-file.css"
                                };
                testable.Mock<IFileWrapper>().Setup(x => x.GetFiles("dir")).Returns(files);
                testable.Mock<IUriBuilder>().Setup(x => x.ParseKey(files[0])).Returns(guid1);
                testable.Mock<IUriBuilder>().Setup(x => x.ParseKey(files[1])).Returns(guid2);

                var result = testable.ClassUnderTest.GetSavedUrls();

                Assert.Equal(2, result.Count);
                Assert.True(result[guid1] == "url1");
                Assert.True(result[guid2] == "url2");
            }
        }

        public class RegisterDeleteCsAction
        {
            [Fact]
            public void WillRegisterAction()
            {
                var testable = new TestableLocalDiskStore();
                Guid key = new Guid();
                var expectedGuid = Guid.NewGuid();
                testable.ClassUnderTest.CssDeleted += (x => key = x);

                testable.ClassUnderTest.TriggerChange("delete", expectedGuid);

                Assert.Equal(expectedGuid, key);
            }
        }

        public class RegisterAddCsAction
        {
            [Fact]
            public void WillRegisterAction()
            {
                var testable = new TestableLocalDiskStore();
                Guid key = new Guid();
                var expectedGuid = Guid.NewGuid();
                testable.ClassUnderTest.CssAded += ((x,y) => key = x);

                testable.ClassUnderTest.TriggerChange("add", expectedGuid);

                Assert.Equal(expectedGuid, key);
            }
        }
    }
}
