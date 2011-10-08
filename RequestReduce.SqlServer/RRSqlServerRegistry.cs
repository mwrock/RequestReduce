using RequestReduce.IOC;
using RequestReduce.Store;
using StructureMap.Configuration.DSL;

namespace RequestReduce.SqlServer
{
    public class RRSqlServerRegistry : Registry
    {
        public RRSqlServerRegistry()
        {
            For<IStore>().LifecycleIs(new RRHybridLifecycle())
                         .Use<SqlServerStore>()
                         .Ctor<IStore>()
                         .Is(y => y.GetInstance<DbDiskCache>());

            For<IFileRepository>().Use<FileRepository>();
        }
    }
}
