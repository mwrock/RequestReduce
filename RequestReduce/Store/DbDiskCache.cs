using System;
using System.Linq;
using System.Threading;
using System.Collections.Concurrent;
using RequestReduce.Configuration;
using RequestReduce.Utilities;

namespace RequestReduce.Store
{
    public class DbDiskCache : LocalDiskStore
    {
        protected Timer timer = null;
        protected ConcurrentDictionary<string, DateTime> fileList = new ConcurrentDictionary<string, DateTime>();

        public DbDiskCache(IFileWrapper fileWrapper, IRRConfiguration configuration, IUriBuilder uriBuilder) : base(fileWrapper, configuration, uriBuilder)
        {
            fileWrapper.DeleteDirectory(configuration.SpritePhysicalPath);
            RRTracer.Trace("DbFileCache deleting physical directory");
            const int interval = 1000*60*5;
            timer = new Timer(PurgeOldFiles, null, interval, interval);
        }

        protected virtual void PurgeOldFiles(object state)
        {
            var date = DateTime.MinValue;
            var oldEntries = fileList.Where(x => DateTime.Now.Subtract(x.Value).TotalMinutes > 5);
            foreach (var entry in oldEntries)
            {
                fileWrapper.DeleteFile(GetFileNameFromConfig(entry.Key));
                fileList.TryRemove(entry.Key, out date);
            }
        }

        public override void Save(byte[] content, string url, string originalUrls)
        {
            base.Save(content, url, originalUrls);
            fileList.TryAdd(url, DateTime.Now);
        }

        public override bool SendContent(string url, System.Web.HttpResponseBase response)
        {
            if(fileList.ContainsKey(url))
                return base.SendContent(url, response);

            return false;
        }

        public override void Dispose()
        {
            base.Dispose();
            if (timer != null)
                timer.Dispose();
        }

        protected override void SetupWatcher()
        {
            
        }
    }
}
