using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using RequestReduce.Configuration;
using RequestReduce.Store;
using RequestReduce.Utilities;
using UriBuilder = RequestReduce.Utilities.UriBuilder;
using Xunit;
using RequestReduce.Reducer;
using RequestReduce.ResourceTypes;

namespace RequestReduce.Facts.Store
{
    public class RepositoryFacts
    {

        class FakeFileRepository : FileRepository
        {
            public FakeFileRepository(IRRConfiguration config)
                : base(config)
            {
                Database.SetInitializer<RequestReduceContext>(new DropCreateDatabaseAlways<RequestReduceContext>());
                Context.Database.Initialize(true);
            }
        }

        class TestableRepository : Testable<FakeFileRepository>
        {
            public TestableRepository()
            {
                Mock<IRRConfiguration>().Setup(x => x.ConnectionStringName).Returns("RRConnection");
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
                Assert.Equal(file.LastUpdated, savedFile.LastUpdated);
            }

            [Fact]
            public void WillUpdateContentAndLastUpdatedTime()
            {
                var testable = new TestableRepository();
                var id = Guid.NewGuid();
                var file = new RequestReduceFile()
                {
                    Content = new byte[] { 1 },
                    FileName = "fileName",
                    Key = Guid.NewGuid(),
                    LastUpdated = new DateTime(2010, 1, 1),
                    OriginalName = "originalName",
                    RequestReduceFileId = id
                };
                testable.ClassUnderTest.Save(file);
                var file2 = new RequestReduceFile()
                {
                    Content = new byte[] { 2 },
                    FileName = "fileName",
                    Key = Guid.NewGuid(),
                    LastUpdated = new DateTime(2011, 1, 1),
                    OriginalName = "originalName",
                    RequestReduceFileId = id
                };

                testable.ClassUnderTest.Save(file2);

                var savedFile = testable.ClassUnderTest[id];
                Assert.Equal(2, savedFile.Content[0]);
                Assert.Equal(new DateTime(2011, 1, 1), savedFile.LastUpdated);
            }

        }

        public class GetActiveFiles
        {
            [Fact]
            public void WillReturnListOfCssFiles()
            {
                var testable = new TestableRepository();
                var builder = new RequestReduce.Utilities.UriBuilder(testable.Mock<IRRConfiguration>().Object);
                var id = Guid.NewGuid();
                var id2 = Guid.NewGuid();
                var file = new RequestReduceFile()
                {
                    Content = new byte[] { 1 },
                    FileName = builder.BuildResourceUrl<CssResource>(id, new byte[] { 1 }),
                    Key = id,
                    LastUpdated = DateTime.Now,
                    OriginalName = "originalName",
                    RequestReduceFileId = Hasher.Hash(new byte[] { 1 })
                };
                var file2 = new RequestReduceFile()
                {
                    Content = new byte[] { 1 },
                    FileName = builder.BuildSpriteUrl(id, new byte[] { 2 }),
                    Key = id,
                    LastUpdated = DateTime.Now,
                    RequestReduceFileId = Hasher.Hash(new byte[] { 2 })
                };
                var file3 = new RequestReduceFile()
                {
                    Content = new byte[] { 1 },
                    FileName = builder.BuildResourceUrl<CssResource>(id2, new byte[] { 3 }),
                    Key = id2,
                    LastUpdated = DateTime.Now,
                    OriginalName = "originalName2",
                    RequestReduceFileId = Hasher.Hash(new byte[] { 3 })
                };
                testable.ClassUnderTest.Save(file);
                testable.ClassUnderTest.Save(file2);
                testable.ClassUnderTest.Save(file3);

                var result = testable.ClassUnderTest.GetActiveFiles();

                Assert.Equal(2, result.Count());
                Assert.True(result.Contains(file.FileName));
                Assert.True(result.Contains(file3.FileName));
            }

            [Fact]
            public void WillNotReturnExpiredKeys()
            {
                var testable = new TestableRepository();
                var builder = new RequestReduce.Utilities.UriBuilder(testable.Mock<IRRConfiguration>().Object);
                var id = Guid.NewGuid();
                var id2 = Guid.NewGuid();
                var file = new RequestReduceFile()
                {
                    Content = new byte[] { 1 },
                    FileName = builder.BuildResourceUrl<CssResource>(id, new byte[] { 1 }),
                    Key = id,
                    LastUpdated = DateTime.Now,
                    OriginalName = "originalName",
                    RequestReduceFileId = Hasher.Hash(new byte[] { 1 }),
                    IsExpired = true
                };
                var file2 = new RequestReduceFile()
                {
                    Content = new byte[] { 1 },
                    FileName = builder.BuildResourceUrl<CssResource>(id2, new byte[] { 2 }),
                    Key = id2,
                    LastUpdated = DateTime.Now,
                    OriginalName = "originalName2",
                    RequestReduceFileId = Hasher.Hash(new byte[] { 2 })
                };
                testable.ClassUnderTest.Save(file);
                testable.ClassUnderTest.Save(file2);

                var result = testable.ClassUnderTest.GetActiveFiles();

                Assert.Equal(1, result.Count());
                Assert.True(result.Contains(file2.FileName));
            }

            [Fact]
            public void WillReturnMostRecentActiveEntryPerKeyKey()
            {
                var testable = new TestableRepository();
                var builder = new RequestReduce.Utilities.UriBuilder(testable.Mock<IRRConfiguration>().Object);
                var id = Guid.NewGuid();
                var id2 = Guid.NewGuid();
                var file = new RequestReduceFile()
                {
                    Content = new byte[] { 1 },
                    FileName = builder.BuildResourceUrl<CssResource>(id, new byte[] { 1 }),
                    Key = id,
                    LastUpdated = DateTime.Now,
                    OriginalName = "originalName",
                    RequestReduceFileId = Hasher.Hash(new byte[] { 1 }),
                    IsExpired = true
                };
                var file2 = new RequestReduceFile()
                {
                    Content = new byte[] { 1 },
                    FileName = builder.BuildResourceUrl<CssResource>(id, new byte[] { 2 }),
                    Key = id,
                    LastUpdated = DateTime.Now.Subtract(new TimeSpan(0, 0, 2)),
                    OriginalName = "originalName2",
                    RequestReduceFileId = Hasher.Hash(new byte[] { 2 })
                };
                var file3 = new RequestReduceFile()
                {
                    Content = new byte[] { 1 },
                    FileName = builder.BuildResourceUrl<CssResource>(id, new byte[] { 3 }),
                    Key = id,
                    LastUpdated = DateTime.Now.Subtract(new TimeSpan(0, 0, 3)),
                    OriginalName = "originalName2",
                    RequestReduceFileId = Hasher.Hash(new byte[] { 3 })
                };
                var file4 = new RequestReduceFile()
                {
                    Content = new byte[] { 1 },
                    FileName = builder.BuildResourceUrl<CssResource>(id, new byte[] { 4 }),
                    Key = id2,
                    LastUpdated = DateTime.Now,
                    OriginalName = "originalName2",
                    RequestReduceFileId = Hasher.Hash(new byte[] { 4 })
                };
                testable.ClassUnderTest.Save(file);
                testable.ClassUnderTest.Save(file2);
                testable.ClassUnderTest.Save(file3);
                testable.ClassUnderTest.Save(file4);

                var result = testable.ClassUnderTest.GetActiveFiles();

                Assert.Equal(2, result.Count());
                Assert.True(result.Contains(file2.FileName));
                Assert.True(result.Contains(file4.FileName));
            }

        }

        public class GetFilesFromKey
        {
            [Fact]
            public void WillPullTheFilesWithTheSpecifiedKey()
            {
                var testable = new TestableRepository();
                var id = Guid.NewGuid();
                var id2 = Guid.NewGuid();
                var file = new RequestReduceFile()
                {
                    Content = new byte[] { 1 },
                    FileName = new CssResource().FileName,
                    Key = id,
                    LastUpdated = DateTime.Now,
                    OriginalName = "originalName",
                    RequestReduceFileId = Guid.NewGuid()
                };
                var file2 = new RequestReduceFile()
                {
                    Content = new byte[] { 1 },
                    FileName = new CssResource().FileName,
                    Key = id,
                    LastUpdated = DateTime.Now,
                    RequestReduceFileId = Guid.NewGuid()
                };
                var file3 = new RequestReduceFile()
                {
                    Content = new byte[] { 1 },
                    FileName = new CssResource().FileName,
                    Key = id2,
                    LastUpdated = DateTime.Now,
                    OriginalName = "originalName2",
                    RequestReduceFileId = Guid.NewGuid()
                };
                testable.ClassUnderTest.Save(file);
                testable.ClassUnderTest.Save(file2);
                testable.ClassUnderTest.Save(file3);

                var result = testable.ClassUnderTest.GetFilesFromKey(id);

                Assert.Equal(2, result.Count());
                Assert.True(result.All(x => x.Key == id));
                Assert.True(result.Contains(file));
                Assert.True(result.Contains(file2));
            }
        }

        public class GetUrlByKey
        {
            [Fact]
            public void WillGetCssUrlFromDb()
            {
                var testable = new TestableRepository();
                var id = Guid.NewGuid();
                var file = new RequestReduceFile()
                {
                    Content = new byte[] { 1 },
                    FileName = "fileName1" + new CssResource().FileName,
                    Key = id,
                    LastUpdated = new DateTime(2010, 1, 1),
                    OriginalName = "originalName",
                    RequestReduceFileId = Guid.NewGuid()
                };
                testable.ClassUnderTest.Save(file);
                var file2 = new RequestReduceFile()
                {
                    Content = new byte[] { 2 },
                    FileName = "fileName2" + new CssResource().FileName,
                    Key = Guid.NewGuid(),
                    LastUpdated = new DateTime(2011, 1, 1),
                    OriginalName = "originalName",
                    RequestReduceFileId = Guid.NewGuid()
                };
                testable.ClassUnderTest.Save(file2);

                var result = testable.ClassUnderTest.GetActiveUrlByKey(id, typeof(CssResource));

                Assert.Equal(file.FileName, result);
            }
        }

        [Fact]
        public void WillGetNonExpiredCssUrlFromDb()
        {
            var testable = new TestableRepository();
            var id = Guid.NewGuid();
            var file = new RequestReduceFile()
            {
                Content = new byte[] { 1 },
                FileName = "fileName1" + new CssResource().FileName,
                Key = id,
                LastUpdated = new DateTime(2010, 1, 1),
                OriginalName = "originalName",
                RequestReduceFileId = id,
                IsExpired = true
            };
            testable.ClassUnderTest.Save(file);
            var file2 = new RequestReduceFile()
            {
                Content = new byte[] { 2 },
                FileName = "fileName2" + new CssResource().FileName,
                Key = id,
                LastUpdated = new DateTime(2011, 1, 1),
                OriginalName = "originalName",
                RequestReduceFileId = Guid.NewGuid()
            };
            testable.ClassUnderTest.Save(file2);

            var result = testable.ClassUnderTest.GetActiveUrlByKey(id, typeof(CssResource));

            Assert.Equal(file2.FileName, result);
        }


    }
}
