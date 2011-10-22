using RequestReduce.Store;
using StructureMap.Configuration.DSL;

namespace RequestReduce.IOC
{
    public class RRLocalStoreRegistry : Registry
    {
        public RRLocalStoreRegistry()
        {
            For<IStore>().Singleton()
                         .Use<LocalDiskStore>();
        }
    }
}
