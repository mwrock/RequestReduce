using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RequestReduce.Store
{
    public class SqlServerStore : IStore
    {
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Save(byte[] content, string url)
        {
            throw new NotImplementedException();
        }

        public byte[] GetContent(string url)
        {
            throw new NotImplementedException();
        }

        public Stream OpenStream(string url)
        {
            throw new NotImplementedException();
        }

        public IDictionary<Guid, string> GetSavedUrls()
        {
            throw new NotImplementedException();
        }

        public void RegisterDeleteCssAction(DeleeCsAction deleteAction)
        {
            throw new NotImplementedException();
        }

        public void RegisterAddCssAction(AddCssAction addAction)
        {
            throw new NotImplementedException();
        }
    }
}
