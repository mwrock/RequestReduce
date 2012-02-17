using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RequestReduce.SqlServer
{
    public interface IPetaPocoFileRepository : IPetaPocoRepository
    {
        IEnumerable<string> GetActiveFiles();
        IEnumerable<RequestReduceFile> GetFilesFromKey(Guid key);
        string GetActiveUrlByKey(Guid key, Type resourceType);
    }

    public class PetaPocoFileRepository : PetaPocoRepository, IPetaPocoFileRepository
    {
        public IEnumerable<string> GetActiveFiles()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<RequestReduceFile> GetFilesFromKey(Guid key)
        {
            throw new NotImplementedException();
        }

        public string GetActiveUrlByKey(Guid key, Type resourceType)
        {
            throw new NotImplementedException();
        }
    }
}
