using System;
using System.Reflection;
using System.Text;
using System.Web;
using StructureMap.Exceptions;
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

                x.For<IReducingQueue>().Singleton().Use<ReducingQueue>();
                x.For<IReductionRepository>().Singleton().Use<ReductionRepository>();
                x.For<IWuQuantizer>().Singleton().Use<WuQuantizer>();
                
                x.For<HttpContextBase>().Use(() => HttpContext.Current == null 
                                                    ? null 
                                                    : new HttpContextWrapper(HttpContext.Current));

                x.For<AbstractFilter>().Use(y => HttpContext.Current == null
                                                    ? new ResponseFilter(null, Encoding.UTF8, y.GetInstance<IResponseTransformer>())
                                                    : new ResponseFilter(HttpContext.Current.Response.Filter, HttpContext.Current.Response.ContentEncoding, y.GetInstance<IResponseTransformer>()));
            });

            LoadAllReducers(initContainer);
            LoadAllResourceTypes(initContainer);
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
                y.AddAllTypesOf<IReducer>();
                y.WithDefaultConventions();
            }));
        }

        private static void LoadAllResourceTypes(IContainer initContainer)
        {
            initContainer.Configure(x => x.Scan(y =>
            {
                y.Assembly("RequestReduce");
                y.IncludeNamespace("RequestReduce.Utilities");
                y.IncludeNamespace("RequestReduce.Configuration");
                y.IncludeNamespace("RequestReduce.ResourceTypes");
                y.AddAllTypesOf<IResourceType>();
                y.With(new SingletonConvention());
            }));
        }

        public static void LoadAppropriateStoreRegistry(IContainer initContainer)
        {
            var store = initContainer.GetInstance<IRRConfiguration>().ContentStore;
            if (store == Configuration.Store.SqlServerStore)
            {
                try
                {
                    Assembly sqlAssembly = Assembly.Load("RequestReduce.SqlServer");
                    initContainer.Configure(x => x.Scan(y =>
                    {
                        y.Assembly(sqlAssembly);
                        y.LookForRegistries();
                    }));
                }
                catch
                {
                    throw new StructureMapConfigurationException("Unable to load RequestReduce.SqlServer assembly.");
                }
            }
            else
                initContainer.Configure(x => x.AddRegistry<RRLocalStoreRegistry>());
        }

        public static IContainer Current
        {
            get { return container ?? (container = InitContainer()); }
            set { container = value; }
        }

    }
}