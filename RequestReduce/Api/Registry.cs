using System;
using System.Collections.Generic;
using System.Web;
using RequestReduce.Configuration;
using RequestReduce.IOC;
using RequestReduce.Utilities;

namespace RequestReduce.Api
{
    public static class Registry
    {

        static Registry()
        {
            HandlerMaps = new List<Func<Uri, IHttpHandler>>();
        }

        public static Action<Exception> CaptureErrorAction { get; set; }

        public delegate string UrlTransformFunc(string originalabsoluteUrl, string urlWithContentHost);
        public delegate string ContextUrlTransformFunc(HttpContextBase httpRequest, string originalabsoluteUrl, string urlWithContentHost);
        [Obsolete("Use RequestReduce.Api.Registry.UrlTransformer")]
        public static UrlTransformFunc AbsoluteUrlTransformer { get; set; }
        public static ContextUrlTransformFunc UrlTransformer { get; set; }
        internal static IList<Func<Uri, IHttpHandler>> HandlerMaps { get; private set; }

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
    }
}
