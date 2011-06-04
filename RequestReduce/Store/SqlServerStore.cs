using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using RequestReduce.Utilities;
using UriBuilder = RequestReduce.Utilities.UriBuilder;

namespace RequestReduce.Store
{
    public class SqlServerStore : IStore
    {
        private readonly IUriBuilder uriBuilder;
        private readonly IRepository<RequestReduceFile> repository;
        private readonly IStore fileStore;

        public SqlServerStore(IUriBuilder uriBuilder, IRepository<RequestReduceFile> repository, IStore fileStore)
        {
            this.uriBuilder = uriBuilder;
            this.repository = repository;
            this.fileStore = fileStore;
        }

        public void Dispose()
        {
        }

        public void Save(byte[] content, string url, string originalUrls)
        {
            var fileName = uriBuilder.ParseFileName(url);
            var key = uriBuilder.ParseKey(url);
            var id = Hasher.Hash(key + fileName);
            var file = new RequestReduceFile()
                           {
                               Content = content,
                               LastAccessed = DateTime.Now,
                               LastUpdated = DateTime.Now,
                               FileName = fileName,
                               Key = key,
                               RequestReduceFileId = id,
                               OriginalName = originalUrls
                           };
            repository.Save(file);
            fileStore.Save(content, url, originalUrls);
            CssAded(key, url);
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

        public event DeleeCsAction CssDeleted;
        public event AddCssAction CssAded;

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
