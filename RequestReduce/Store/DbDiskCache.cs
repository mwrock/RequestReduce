using System;
using System.Linq;
using System.Threading;
using System.Collections.Concurrent;
using RequestReduce.Configuration;
using RequestReduce.Module;
using RequestReduce.Utilities;

namespace RequestReduce.Store
{
    public class DbDiskCache : LocalDiskStore
    {
        protected Timer timer = null;
        protected ConcurrentDictionary<string, DateTime> fileList = new ConcurrentDictionary<string, DateTime>();

        public DbDiskCache(IFileWrapper fileWrapper, IRRConfiguration configuration, IUriBuilder uriBuilder) : base(fileWrapper, configuration, uriBuilder)
        {
            const int interval = 1000*60*5;
            timer = new Timer(PurgeOldFiles, null, 0, interval);
        }

        protected virtual void PurgeOldFiles(object state)
        {
            var date = DateTime.MinValue;
            var oldEntries = fileList.Where(x => DateTime.Now.Subtract(x.Value).TotalMinutes > 5);
            foreach (var entry in oldEntries)
            {
                var file = GetFileNameFromConfig(entry.Key);
                RRTracer.Trace("purging old file: {0}", file);
                try
                {
                    fileWrapper.DeleteFile(file);
                    fileList.TryRemove(entry.Key, out date);
                }
                catch (Exception ex)
                {
                    if (RequestReduceModule.CaptureErrorAction != null)
                       RequestReduceModule.CaptureErrorAction(ex);
                }
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
            {
                fileList[url] = DateTime.Now;
                return base.SendContent(url, response);
            }

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
