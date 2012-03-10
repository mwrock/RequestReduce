using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RequestReduce.Api;
using RequestReduce.Module;
using RequestReduce.Store;
using RequestReduce.Utilities;
using RequestReduce.IOC;

namespace RequestReduce.SqlServer
{
    public class SqlServerStore : IStore
    {
        private readonly IUriBuilder uriBuilder;
        private readonly IFileRepository repository;
        internal readonly IStore FileStore;
        private readonly IReductionRepository reductionRepository;

        public SqlServerStore(IUriBuilder uriBuilder, IFileRepository repository, LocalDiskStore fileStore, IReductionRepository reductionRepository)
        {
            RRTracer.Trace("Sql Server Store Created.");
            this.uriBuilder = uriBuilder;
            this.repository = repository;
            FileStore = fileStore;
            this.reductionRepository = reductionRepository;
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
            reductionRepository.RemoveReduction(keyGuid);
        }

        public void Dispose()
        {
            repository.Dispose();
            RRTracer.Trace("Sql Server Store Disposed.");
        }

        public void Save(byte[] content, string url, string originalUrls)
        {
            RRTracer.Trace("Saving {0} to db.", url);
            var fileName = uriBuilder.ParseFileName(url);
            var key = uriBuilder.ParseKey(url);
            var id = Hasher.Hash(fileName);
            var file = repository.SingleOrDefault<RequestReduceFile>(id) ?? new RequestReduceFile();
            file.Content = content;
            file.LastUpdated = DateTime.Now;
            file.FileName = fileName;
            file.Key = key;
            file.RequestReduceFileId = id;
            file.OriginalName = originalUrls;
            file.IsExpired = false;
            FileStore.Save(content, url, originalUrls);
            repository.Save(file);
            if (!url.ToLower().EndsWith(".png"))
                reductionRepository.AddReduction(key, url);
            RRTracer.Trace("{0} saved to db.", url);
        }

        public bool SendContent(string url, HttpResponseBase response)
        {
            if (FileStore.SendContent(url, response))
                return true;

            var key = uriBuilder.ParseKey(url);
            var id = Hasher.Hash(uriBuilder.ParseFileName(url));
            var file = repository.SingleOrDefault<RequestReduceFile>(id);

            if(file != null)
            {
                response.BinaryWrite(file.Content);
                try
                {
                    FileStore.Save(file.Content, url, null);
                }
                catch(Exception ex)
                {
                    var message = string.Format("could not save {0}", url);
                    var wrappedException =
                        new ApplicationException(message, ex);
                    RRTracer.Trace(message);
                    RRTracer.Trace(ex.ToString());
                    if (Registry.CaptureErrorAction != null)
                        Registry.CaptureErrorAction(wrappedException);
                }
                RRTracer.Trace("{0} transmitted from db.", url);
                if (file.IsExpired)
                    reductionRepository.RemoveReduction(key);
                return true;
            }

            RRTracer.Trace("{0} not found on file or db.", url);
            reductionRepository.RemoveReduction(key);
            return false;
        }

        public IDictionary<Guid, string> GetSavedUrls()
        {
            RRTracer.Trace("SqlServerStore Looking for previously saved content.");
            var files = repository.GetActiveFiles();
            return files.ToDictionary(file => uriBuilder.ParseKey(file), file => uriBuilder.BuildResourceUrl(uriBuilder.ParseKey(file), uriBuilder.ParseSignature(file), RRContainer.GetAllResourceTypes().SingleOrDefault(x => file.EndsWith(x.FileName)).GetType()));
        }


        public string GetUrlByKey(Guid keyGuid, Type resourceType)
        {
            var fileName = repository.GetActiveUrlByKey(keyGuid, resourceType);
            return fileName == null ? null : uriBuilder.BuildResourceUrl(uriBuilder.ParseKey(fileName), uriBuilder.ParseSignature(fileName), resourceType);
        }
    }
}
