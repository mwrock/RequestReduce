using System;
using RequestReduce.Configuration;
using RequestReduce.IOC;
using RequestReduce.Utilities;

namespace RequestReduce.Api
{
    public static class Registry
    {
        public static Action<Exception> CaptureErrorAction { get; set; }
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
