using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using RequestReduce.Utilities;
using UriBuilder = RequestReduce.Utilities.UriBuilder;

namespace RequestReduce.Store
{
    public class SqlServerStore : IStore
    {
        private readonly IUriBuilder uriBuilder;
        private readonly IFileRepository repository;
        private readonly IStore fileStore;
        public event DeleeCsAction CssDeleted;
        public event AddCssAction CssAded;

        public SqlServerStore(IUriBuilder uriBuilder, IFileRepository repository, IStore fileStore)
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
            if(CssAded != null && !url.ToLower().EndsWith(".png"))
                CssAded(key, url);
        }

        public bool SendContent(string url, HttpResponseBase response)
        {
            if (fileStore.SendContent(url, response))
                return true;

            var fileName = uriBuilder.ParseFileName(url);
            var key = uriBuilder.ParseKey(url);
            var id = Hasher.Hash(key + fileName);
            var file = repository[id];

            if(file != null)
            {
                response.BinaryWrite(file.Content);
                fileStore.Save(file.Content, url, null);
                return true;
            }

            if (CssDeleted != null)
                CssDeleted(key);
            return false;
        }

        public IDictionary<Guid, string> GetSavedUrls()
        {
            var keys = repository.GetKeys();
            return keys.ToDictionary(key => key, key => uriBuilder.BuildCssUrl(key));
        }
    }
}
