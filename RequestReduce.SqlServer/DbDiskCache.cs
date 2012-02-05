using System;
using System.Linq;
using System.Threading;
using System.Collections.Concurrent;
using RequestReduce.Api;
using RequestReduce.Configuration;
using RequestReduce.Module;
using RequestReduce.Store;
using RequestReduce.Utilities;

namespace RequestReduce.SqlServer
{
    public class DbDiskCache : LocalDiskStore
    {
        protected Timer timer = null;
        protected ConcurrentDictionary<string, DateTime> fileList = new ConcurrentDictionary<string, DateTime>();

        public DbDiskCache(IFileWrapper fileWrapper, IRRConfiguration configuration, IUriBuilder uriBuilder) : base(fileWrapper, configuration, uriBuilder, null)
        {
            RRTracer.Trace("Creating db disk cache");
            const int interval = 1000*60*5;
            timer = new Timer(PurgeOldFiles, null, 0, interval);
        }

        protected virtual void PurgeOldFiles(object state)
        {
            var oldEntries = fileList.Where(x => DateTime.Now.Subtract(x.Value).TotalMinutes > 5);
            foreach (var entry in oldEntries)
            {
                var file = GetFileNameFromConfig(entry.Key);
                RRTracer.Trace("purging old file: {0}", file);
                try
                {
                    FileWrapper.DeleteFile(file);
                    DateTime date;
                    fileList.TryRemove(entry.Key, out date);
                }
                catch (Exception ex)
                {
                    if (Registry.CaptureErrorAction != null)
                        Registry.CaptureErrorAction(ex);
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
