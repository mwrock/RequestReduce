using System.Collections;
using System.Web;
using StructureMap;
using StructureMap.Pipeline;

namespace RequestReduce.IOC
{
    public class RRHttpContextLifecycle : ILifecycle
    {
        public static readonly string RRITEM_NAME = "RR-STRUCTUREMAP-INSTANCES";

        public void EjectAll()
        {
            FindCache().DisposeAndClear();
        }

        protected virtual IDictionary findHttpDictionary()
        {
            if (!HttpContextLifecycle.HasContext())
                throw new StructureMapException(309);

            return HttpContext.Current.Items;
        }

        public IObjectCache FindCache()
        {
            var dictionary = findHttpDictionary();
            if (!dictionary.Contains(RRITEM_NAME))
            {
                lock (dictionary.SyncRoot)
                {
                    if (!dictionary.Contains(RRITEM_NAME))
                    {
                        var cache = new MainObjectCache();
                        dictionary.Add(RRITEM_NAME, cache);
                        return cache;
                    }
                }
            }
            return (IObjectCache)dictionary[RRITEM_NAME];
        }

        public string Scope
        {
            get { return "RRHttpContextLifecycle"; }
        }
    }
}