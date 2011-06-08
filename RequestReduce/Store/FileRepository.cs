using System;
using System.Collections.Generic;
using System.Linq;
using RequestReduce.Configuration;

namespace RequestReduce.Store
{
    public interface IFileRepository : IRepository<RequestReduceFile>
    {
        IEnumerable<Guid> GetKeys();
    }
    public class FileRepository : Repository<RequestReduceFile>, IFileRepository
    {
        public FileRepository(IRRConfiguration config) : base(config)
        {
        }

        public IEnumerable<Guid> GetKeys()
        {
            return AsQueryable().Select(y => y.Key).Distinct().ToList();
        }
    }
}
