using System;
using System.Data.Entity;
using System.Linq;
using RequestReduce.Configuration;
using RequestReduce.Store;
using Xunit;

namespace RequestReduce.Facts.Store
{
    public class RepositoryFacts
    {
        class TestableRepository : Testable<FileRepository>
        {
            public TestableRepository()
            {
                Database.SetInitializer<RequestReduceContext>(new DropCreateDatabaseAlways<RequestReduceContext>());
                Mock<IRRConfiguration>().Setup(x => x.ConnectionStringName).Returns("RequestReduce");
            }
        }

        public class Save
        {
            [Fact]
            public void WillSaveToDatabase()
            {
                var testable = new TestableRepository();
                var id = Guid.NewGuid();
                var file = new RequestReduceFile()
                               {
                                   Content = new byte[] {1},
                                   FileName = "fileName",
                                   Key = Guid.NewGuid(),
                                   LastAccessed = DateTime.Now,
                                   LastUpdated = DateTime.Now,
                                   OriginalName = "originalName",
                                   RequestReduceFileId = id
                               };

                testable.ClassUnderTest.Save(file);

                var savedFile = testable.ClassUnderTest[id];
                Assert.Equal(file.Content.Length, savedFile.Content.Length);
                Assert.Equal(file.Content[0], savedFile.Content[0]);
                Assert.Equal(file.FileName, savedFile.FileName);
                Assert.Equal(file.Key, savedFile.Key);
                Assert.Equal(file.OriginalName, savedFile.OriginalName);
                Assert.Equal(file.RequestReduceFileId, savedFile.RequestReduceFileId);
                Assert.Equal(file.LastAccessed, savedFile.LastAccessed);
                Assert.Equal(file.LastUpdated, savedFile.LastUpdated);
            }

            [Fact]
            public void WillGracefullyHandleDuplicateSave()
            {
                var testable = new TestableRepository();
                var id = Guid.NewGuid();
                var file = new RequestReduceFile()
                {
                    Content = new byte[] { 1 },
                    FileName = "fileName",
                    Key = Guid.NewGuid(),
                    LastAccessed = DateTime.Now,
                    LastUpdated = DateTime.Now,
                    OriginalName = "originalName",
                    RequestReduceFileId = id
                };

                testable.ClassUnderTest.Save(file);
                Assert.DoesNotThrow(() => testable.ClassUnderTest.Save(file));
            }

        }

        [Fact]
        public void WillReturnDistinctListOfKeys()
        {
            var testable = new TestableRepository();
            var id = Guid.NewGuid();
            var id2 = Guid.NewGuid();
            var file = new RequestReduceFile()
            {
                Content = new byte[] { 1 },
                FileName = "fileName",
                Key = id,
                LastAccessed = DateTime.Now,
                LastUpdated = DateTime.Now,
                OriginalName = "originalName",
                RequestReduceFileId = Guid.NewGuid()
            };
            var file2 = new RequestReduceFile()
            {
                Content = new byte[] { 1 },
                FileName = "fileName2",
                Key = id,
                LastAccessed = DateTime.Now,
                LastUpdated = DateTime.Now,
                RequestReduceFileId = Guid.NewGuid()
            };
            var file3 = new RequestReduceFile()
            {
                Content = new byte[] { 1 },
                FileName = "fileName3",
                Key = id2,
                LastAccessed = DateTime.Now,
                LastUpdated = DateTime.Now,
                OriginalName = "originalName2",
                RequestReduceFileId = Guid.NewGuid()
            };
            testable.ClassUnderTest.Save(file);
            testable.ClassUnderTest.Save(file2);
            testable.ClassUnderTest.Save(file3);

            var result = testable.ClassUnderTest.GetKeys();

            Assert.Equal(2, result.Count());
            Assert.True(result.Contains(id));
            Assert.True(result.Contains(id2));
        }
    }
}
