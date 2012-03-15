using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Web;
using RequestReduce.Utilities;
using StructureMap.Pipeline;
using nQuant;
using RequestReduce.Configuration;
using RequestReduce.Module;
using RequestReduce.Reducer;
using RequestReduce.Store;
using StructureMap;
using RequestReduce.ResourceTypes;

namespace RequestReduce.IOC
{
    public static class RRContainer
    {
        private static IContainer container = InitContainer();

        private static IContainer InitContainer()
        {
            RRTracer.Trace("Initializing container");
            var initContainer = new Container();
            initContainer.Configure(x =>
            {
                x.For<IResourceType>().OnCreationForAll((c, r) => 
                { 
                    if (!r.FileName.Contains("RequestReduce")) 
                        throw new InvalidOperationException ("ResourceType file names must contain the string 'RequestReduce'"); 
                });
                x.For<IResourceType>().Singleton().Add<JavaScriptResource>();
                x.For<IResourceType>().Singleton().Add<CssResource>();
                x.For<JavaScriptResource>().Singleton().Add<JavaScriptResource>();
                x.For<CssResource>().Singleton().Add<CssResource>();
                x.For<IReducingQueue>().Singleton().Use<ReducingQueue>();
                x.For<IReductionRepository>().Singleton().Use<ReductionRepository>();
                x.For<IWuQuantizer>().Singleton().Use<WuQuantizer>();
                x.For<ICssImageTransformer>().Singleton().Use<CssImageTransformer>();
                x.For<IRelativeToAbsoluteUtility>().Use<RelativeToAbsoluteUtility>();

                x.For<HttpContextBase>().Use(() => HttpContext.Current == null 
                                                    ? null 
                                                    : new HttpContextWrapper(HttpContext.Current));

                x.For<AbstractFilter>().Use(y => HttpContext.Current == null
                                                    ? new ResponseFilter(null, null, Encoding.UTF8, y.GetInstance<IResponseTransformer>())
                                                    : new ResponseFilter(y.GetInstance<HttpContextBase>(), HttpContext.Current.Response.Filter, HttpContext.Current.Response.ContentEncoding, y.GetInstance<IResponseTransformer>()));
            });

            LoadAllReducers(initContainer);
            LoadAllSingletons(initContainer);
            LoadAppropriateStoreRegistry(initContainer);

            return initContainer;
        }

        private static void LoadAllReducers(IContainer initContainer)
        {
            initContainer.Configure(x => x.Scan(y =>
            {
                y.Assembly("RequestReduce");
                y.ExcludeNamespace("RequestReduce.Utilities");
                y.ExcludeNamespace("RequestReduce.Configuration");
                y.ExcludeNamespace("RequestReduce.Store");
                y.ExcludeNamespace("RequestReduce.ResourceTypes");
                y.ExcludeType<IReductionRepository>();
                y.ExcludeType<IReducingQueue>();
                y.ExcludeType<ICssImageTransformer>();
                y.AddAllTypesOf<IReducer>();
                y.WithDefaultConventions();
            }));
        }

        private static void LoadAllSingletons(IContainer initContainer)
        {
            initContainer.Configure(x => x.Scan(y =>
            {
                y.Assembly("RequestReduce");
                y.IncludeNamespace("RequestReduce.Utilities");
                y.IncludeNamespace("RequestReduce.Configuration");
                y.ExcludeType<IRelativeToAbsoluteUtility>();
                y.IgnoreStructureMapAttributes();
                y.With(new SingletonConvention());
            }));
        }

        public static void LoadAppropriateStoreRegistry(IContainer initContainer)
        {
            var store = initContainer.GetInstance<IRRConfiguration>().ContentStore;
            if (store == Configuration.Store.SqlServerStore)
            {
                var sqlAssembly = Assembly.Load("RequestReduce.SqlServer");
                initContainer.Configure(x => 
                                            {
                                                x.For(sqlAssembly.GetType("RequestReduce.SqlServer.IFileRepository"))
                                                    .Use(
                                                        sqlAssembly.GetType(
                                                            "RequestReduce.SqlServer.FileRepository"));
                                                var diskStore =
                                                    new ConfiguredInstance(
                                                        sqlAssembly.GetType("RequestReduce.SqlServer.SqlServerStore"));
                                                var diskCache =
                                                    new ConfiguredInstance(
                                                        sqlAssembly.GetType("RequestReduce.SqlServer.DbDiskCache"));
                                                x.For<LocalDiskStore>().Singleton()
                                                    .Use(diskCache);
                                                diskStore.CtorDependency<LocalDiskStore>("fileStore").Is(
                                                    initContainer.GetInstance<LocalDiskStore>());
                                                diskStore.CtorDependency<IUriBuilder>("uriBuilder").Is(
                                                    initContainer.GetInstance<IUriBuilder>());
                                                diskStore.CtorDependency<IReductionRepository>("reductionRepository").Is(
                                                    initContainer.GetInstance<IReductionRepository>());
                                                x.For<IStore>().LifecycleIs(new RRHybridLifecycle())
                                                    .Use(diskStore);
                                            });
            }
            else
                initContainer.Configure(x => x.AddRegistry<RRLocalStoreRegistry>());
        }

        public static IContainer Current
        {
            get { return container ?? (container = InitContainer()); }
            set { container = value; }
        }

        public static IEnumerable<IResourceType> GetAllResourceTypes()
        {
            return Current.GetAllInstances<IResourceType>();
        }

    }
}