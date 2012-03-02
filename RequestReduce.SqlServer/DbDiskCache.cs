using System;
using System.Linq;
using System.Threading;
using RequestReduce.Api;
using RequestReduce.Configuration;
using RequestReduce.Store;
using RequestReduce.Utilities;
using System.Collections.Generic;

namespace RequestReduce.SqlServer
{
    public class DbDiskCache : LocalDiskStore
    {
        protected Timer timer = null;
        protected Dictionary<string, DateTime> fileList = new Dictionary<string, DateTime>();
        private readonly ReaderWriterLockSlim _cacheLock;
        
        public DbDiskCache(IFileWrapper fileWrapper, IRRConfiguration configuration, IUriBuilder uriBuilder) : base(fileWrapper, configuration, uriBuilder, null)
        {
            RRTracer.Trace("Creating db disk cache");
            const int interval = 1000*60*5;
            timer = new Timer(PurgeOldFiles, null, 0, interval);
            _cacheLock = new ReaderWriterLockSlim();
        }

        protected virtual void PurgeOldFiles(object state)
        {
            var oldEntries = new List<KeyValuePair<string, DateTime>>();

            _cacheLock.EnterReadLock();

            try
            {
                oldEntries = fileList.Where(x => DateTime.Now.Subtract(x.Value).TotalMinutes > 5).ToList();
            }
            finally
            {
                _cacheLock.ExitReadLock();
            }

            foreach (var entry in oldEntries)
            {
                var file = GetFileNameFromConfig(entry.Key);
                RRTracer.Trace("purging old file: {0}", file);

                _cacheLock.EnterWriteLock();
                
                try
                {
                    FileWrapper.DeleteFile(file);
                    fileList.Remove(entry.Key);
                }
                catch (Exception ex)
                {
                    if (Registry.CaptureErrorAction != null)
                        Registry.CaptureErrorAction(ex);
                }
                finally
                {
                    _cacheLock.ExitWriteLock();
                }
            }
        }

        public override void Save(byte[] content, string url, string originalUrls)
        {
            base.Save(content, url, originalUrls);

            _cacheLock.EnterWriteLock();

            try
            {    
                fileList.Add(url, DateTime.Now);
            }
            finally
            {
                _cacheLock.ExitWriteLock();
            }
        }

        public override bool SendContent(string url, System.Web.HttpResponseBase response)
        {
            _cacheLock.EnterWriteLock();

            try
            {
                if(fileList.ContainsKey(url))
                {
                    fileList[url] = DateTime.Now;
                    return base.SendContent(url, response);
                }
            }
            finally
            {
                _cacheLock.ExitWriteLock();
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
