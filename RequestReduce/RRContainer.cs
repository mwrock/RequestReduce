﻿using System;
using System.Text;
using System.Web;
using RequestReduce.Configuration;
using RequestReduce.Module;
using RequestReduce.Reducer;
using RequestReduce.Store;
using StructureMap;
using StructureMap.Configuration.DSL;
using StructureMap.Graph;

namespace RequestReduce
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
                                        x.For<IReducingQueue>().Singleton().Use<ReducingQueue>();
                                        x.For<IReductionRepository>().Singleton().Use<ReductionRepository>();
                                        x.For<LocalDiskStore>().Singleton();
                                        x.For<DbDiskCache>().Singleton();
                                        x.For<SqlServerStore>().HybridHttpOrThreadLocalScoped().Use<SqlServerStore>().
                                            Ctor<IStore>().Is(y => y.GetInstance<DbDiskCache>());
                                        x.For<IFileRepository>().Use<FileRepository>();
                                        x.For<IStore>().Use(y =>
                                                                            {
                                                                                switch (
                                                                                    y.GetInstance<IRRConfiguration>().
                                                                                        ContentStore)
                                                                                {
                                                                                    case
                                                                                        Configuration.Store.
                                                                                            SqlServerStore:
                                                                                        return
                                                                                            y.GetInstance
                                                                                                <SqlServerStore>();
                                                                                    case
                                                                                        Configuration.Store.
                                                                                            LocalDiskStore:
                                                                                    default:
                                                                                        return
                                                                                            y.GetInstance
                                                                                                <LocalDiskStore>();
                                                                                }
                                                                            });
                                        x.For<HttpContextBase>().Use(() => HttpContext.Current == null ? null : new HttpContextWrapper(HttpContext.Current));
                                    });
            initContainer.Configure(x => x.Scan(y =>
            {
                y.Assembly("RequestReduce");
                y.ExcludeNamespace("RequestReduce.Utilities");
                y.ExcludeNamespace("RequestReduce.Configuration");
                y.ExcludeNamespace("RequestReduce.Store");
                y.ExcludeType<IReductionRepository>();
                y.ExcludeType<IReducingQueue>();
                y.AddAllTypesOf<IReducer>();
                y.WithDefaultConventions();
            }
            ));
            initContainer.Configure(x => x.Scan(y =>
            {
                y.Assembly("RequestReduce");
                y.IncludeNamespace("RequestReduce.Utilities");
                y.IncludeNamespace("RequestReduce.Configuration");
                y.With(new SingletonConvention());
            }
            ));
            initContainer.Configure(
                x =>
                x.For<AbstractFilter>().Use(
                    y =>
                    HttpContext.Current == null
                        ? new ResponseFilter(null, Encoding.UTF8, y.GetInstance<IResponseTransformer>())
                        : new ResponseFilter(HttpContext.Current.Response.Filter, HttpContext.Current.Response.ContentEncoding, y.GetInstance<IResponseTransformer>())));
           return initContainer;
        }

        public static IContainer Current
        {
            get { return container ?? (container = InitContainer()); }
            set { container = value; }
        }

    }

    public class SingletonConvention : DefaultConventionScanner
    {
        public override void Process(Type type, Registry registry)
        {
            base.Process(type, registry);
            var pluginType = FindPluginType(type);
            if ((pluginType != null))
            {
                registry.For(pluginType).Singleton();
            }
        }
    }
}
