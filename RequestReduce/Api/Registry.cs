using System;
using System.Web;
using RequestReduce.Configuration;
using RequestReduce.Handlers;
using RequestReduce.IOC;
using RequestReduce.Module;
using RequestReduce.Utilities;

namespace RequestReduce.Api
{
    public static class Registry
    {

        public static Action<Exception> CaptureErrorAction { get; set; }

        public delegate string UrlTransformFunc(string originalabsoluteUrl, string urlWithContentHost);
        public delegate string ContextUrlTransformFunc(HttpContextBase httpRequest, string originalabsoluteUrl, string urlWithContentHost);
        public delegate string ResourceFileNameTransformerFunc(string originalFileName);
        [Obsolete("Use RequestReduce.Api.Registry.UrlTransformer")]
        public static UrlTransformFunc AbsoluteUrlTransformer { get; set; }
        public static ContextUrlTransformFunc UrlTransformer { get; set; }
        public static ResourceFileNameTransformerFunc FileNameTransformer { get; set; }
        public static IHandlerFactory HandlerFactory
        {
            get { return RRContainer.Current.GetInstance<IHandlerFactory>(); }
        }

        public static void RegisterMinifier<T>() where T : IMinifier, new()
        {
            RRContainer.Current.Configure(x => x.For<IMinifier>().Use<T>());
        }
        public static void AddFilter(IFilter filter)
        {
            if (!RRContainer.Current.GetAllInstances<IFilter>().Contains(filter))
                RRContainer.Current.Configure(x => x.For<IFilter>().Add(filter));
        }

        public static void AddFilter<T>() where T : IFilter, new()
        {
            RRContainer.Current.Configure(x => x.For<IFilter>().Singleton().Add<T>());
        }

        public static IRRConfiguration Configuration
        {
            get { return RRContainer.Current.GetInstance<IRRConfiguration>(); }
        }

        public static void InstallResponseFilter()
        {
            ResponseFilter.InstallFilter(RRContainer.Current.GetInstance<HttpContextBase>());
        }
    }
}
