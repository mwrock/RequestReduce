using RequestReduce.Configuration;
using RequestReduce.Utilities;

namespace RequestReduce.Store
{
    public class DbDiskCache : LocalDiskStore
    {
        public DbDiskCache(IFileWrapper fileWrapper, IRRConfiguration configuration, IUriBuilder uriBuilder) : base(fileWrapper, configuration, uriBuilder)
        {
        }

        protected override void SetupWatcher()
        {
            
        }
    }
}
