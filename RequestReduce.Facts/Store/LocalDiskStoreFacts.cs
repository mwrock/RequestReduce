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
    }
}
