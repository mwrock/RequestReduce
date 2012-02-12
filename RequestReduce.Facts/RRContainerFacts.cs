using System;
using System.Collections;
using System.Linq;
using System.Threading;
using Moq;
using RequestReduce.Configuration;
using RequestReduce.IOC;
using RequestReduce.Module;
using RequestReduce.ResourceTypes;
using RequestReduce.SqlServer;
using RequestReduce.Store;
using RequestReduce.Utilities;
using StructureMap;
using Xunit;

namespace RequestReduce.Facts
{
    public class RRContainerFacts : IDisposable
    {
        public class BadResource : IResourceType
        {
            public string FileName
            {
                get { return "badfilename"; }
            }

            public System.Collections.Generic.IEnumerable<string> SupportedMimeTypes
            {
                get { throw new System.NotImplementedException(); }
            }

            public int Bundle(string resource)
            {
                throw new System.NotImplementedException();
            }

            public string TransformedMarkupTag(string url, int bundle)
            {
                throw new System.NotImplementedException();
            }

            public System.Text.RegularExpressions.Regex ResourceRegex
            {
                get { return null; }
            }


            public System.Func<string, string, bool> TagValidator { get; set; }


            public bool IsLoadDeferred(int bundle)
            {
                throw new NotImplementedException();
            }


            public bool IsDynamicLoad(int bundle)
            {
                throw new NotImplementedException();
            }
        }

        [Fact]
        public void ContainerIsValid()
        {
            RRContainer.Current.AssertConfigurationIsValid();
        }

        [Fact]
        public void WillThowIfResourceTypeMissingRequestReduceInFileName()
        {
            RRContainer.Current.Configure(x => x.Scan( y => {
                y.Assembly("RequestReduce.Facts");
                y.AddAllTypesOf<IResourceType>();
            }));

            var ex = Record.Exception(() => RRContainer.Current.AssertConfigurationIsValid());

            Assert.NotNull(ex);
        }

        [Fact]
        public void StoreIsLocalDiskWhenConfigurationIsLocalDisk()
        {
            var container = new Container();
            var config = Mock.Of<IRRConfiguration>(c => c.ContentStore == Configuration.Store.LocalDiskStore);
            container.Configure(x =>
            {
                x.For<IRRConfiguration>().Use(config);
                x.For<IFileWrapper>().Use(Mock.Of<IFileWrapper>());
                x.For<IUriBuilder>().Use(Mock.Of<IUriBuilder>());
                x.For<IReductionRepository>().Use(Mock.Of<IReductionRepository>());
            });
            RRContainer.LoadAppropriateStoreRegistry(container);

            var result = container.GetInstance<IStore>();
            
            Assert.IsType<LocalDiskStore>(result);
        }

        [Fact]
        public void StoreIsSqlServerWhenConfigurationIsSqlServerAndAssemblyExists()
        {
            var container = new Container();
            var config = new Mock<IRRConfiguration>();
            config.Setup(c => c.ContentStore).Returns(Configuration.Store.SqlServerStore);
            config.Setup(x => x.ConnectionStringName).Returns("RRConnection");
            container.Configure(x => x.For<IRRConfiguration>().Use(config.Object));
            container.Configure(x =>
            {
                x.For<IUriBuilder>().Use(Mock.Of<IUriBuilder>());
                x.For<IFileRepository>().Use(Mock.Of<IFileRepository>());
                x.For<IReductionRepository>().Use(Mock.Of<IReductionRepository>());
                x.For<IFileWrapper>().Use(Mock.Of<IFileWrapper>());
            });
            RRContainer.LoadAppropriateStoreRegistry(container);

            var result = container.GetInstance<IStore>();

            Assert.IsType<SqlServerStore>(result);
        }

        [Fact]
        public void ResouceTypesAreSingletons()
        {
            var resources1 = RRContainer.GetAllResourceTypes();
            var resources2 = RRContainer.GetAllResourceTypes();

            var r1 = resources1.First(x => x is JavaScriptResource);
            var r2 = resources2.First(x => x is JavaScriptResource);
            Assert.Equal(r1,r2);
        }

        [Fact]
        public void DbDiskCacheIsSingleton()
        {
            var container = RRContainer.Current;
            var config = new Mock<IRRConfiguration>();
            config.Setup(c => c.ContentStore).Returns(Configuration.Store.SqlServerStore);
            config.Setup(x => x.ConnectionStringName).Returns("RRConnection");
            container.Configure(x => x.For<IRRConfiguration>().Use(config.Object));
            RRContainer.LoadAppropriateStoreRegistry(container);
            SqlServerStore store1 = null;
            SqlServerStore store2 = null;
            var thread1 = new Thread(TestThreadStart){IsBackground = true};
            var thread2 = new Thread(TestThreadStart) { IsBackground = true };
            var array1 = new ArrayList { container, store1 };
            var array2 = new ArrayList { container, store2 };

            lock (container)
            {
                thread1.Start(array1);
                Monitor.Wait(container, 10000);
                thread2.Start(array2);
                Monitor.Wait(container, 10000);
            }

            Assert.NotSame(array1[1], array2[1]);
            Assert.Same((array1[1] as SqlServerStore).FileStore as DbDiskCache, (array2[1] as SqlServerStore).FileStore as DbDiskCache);
            RRContainer.Current = null;
        }

        private void TestThreadStart(object containerAndStore)
        {
            var container = (containerAndStore as ArrayList)[0] as Container;
            lock (container)
            {
                (containerAndStore as ArrayList)[1] = container.GetInstance<IStore>() as SqlServerStore;
            }
        }

        public void Dispose()
        {
            RRContainer.Current.Dispose();
            RRContainer.Current = null;
        }
    }
}
