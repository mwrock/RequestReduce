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

        public void Flush(Guid keyGuid)
        {
            if(keyGuid == Guid.Empty)
            {
                var urls = GetSavedUrls();
                foreach (var key in urls.Keys)
                    Flush(key);
            }

            var files = repository.GetFilesFromKey(keyGuid);
            foreach (var file in files)
            {
                file.IsExpired = true;
                repository.Save(file);
            }
            if (CssDeleted != null)
                CssDeleted(keyGuid);
        }

        public void Dispose()
        {
            fileStore.Dispose();
            RRTracer.Trace("Sql Server Store Disposed.");
        }

        public void Save(byte[] content, string url, string originalUrls)
        {
            RRTracer.Trace("Saving {0} to db.", url);
            var fileName = uriBuilder.ParseFileName(url);
            var key = uriBuilder.ParseKey(url);
            var id = Guid.Parse(uriBuilder.ParseSignature(url));
            var file = new RequestReduceFile()
                           {
                               Content = content,
                               LastUpdated = DateTime.Now,
                               FileName = fileName,
                               Key = key,
                               RequestReduceFileId = id,
                               OriginalName = originalUrls
                           };
            fileStore.Save(content, url, originalUrls);
            if(CssAded != null && !url.ToLower().EndsWith(".png"))
                CssAded(key, url);
            repository.Save(file);
            RRTracer.Trace("{0} saved to db.", url);
        }

        public bool SendContent(string url, HttpResponseBase response)
        {
            if (fileStore.SendContent(url, response))
                return true;

            var key = uriBuilder.ParseKey(url);
            var id = Guid.Parse(uriBuilder.ParseSignature(url));
            var file = repository[id];

            if(file != null)
            {
                response.BinaryWrite(file.Content);
                fileStore.Save(file.Content, url, null);
                RRTracer.Trace("{0} transmitted from db.", url);
                if (file.IsExpired && CssDeleted != null)
                    CssDeleted(key);
                return true;
            }

            RRTracer.Trace("{0} not found on file or db.", url);
            if (CssDeleted != null)
                CssDeleted(key);
            return false;
        }

        public IDictionary<Guid, string> GetSavedUrls()
        {
            RRTracer.Trace("SqlServerStore Looking for previously saved content.");
            var files = repository.GetActiveCssFiles();
            return files.ToDictionary(file => uriBuilder.ParseKey(file), file => uriBuilder.BuildCssUrl(uriBuilder.ParseKey(file), uriBuilder.ParseSignature(file)));
        }
    }
}
