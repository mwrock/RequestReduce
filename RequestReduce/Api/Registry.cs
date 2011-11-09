using System;
using RequestReduce.IOC;
using RequestReduce.Utilities;

namespace RequestReduce.Api
{
    public static class Registry
    {
        public static Action<Exception> CaptureErrorAction { get; set; }
        public static void RegisterMinifier(IMinifier minifier)
        {
            if (minifier == null)
                throw new ArgumentNullException("minifier", "Cannot register a null minifier");

            RRContainer.Current.Configure(x => x.For<IMinifier>().Use(minifier));
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
    }
}
