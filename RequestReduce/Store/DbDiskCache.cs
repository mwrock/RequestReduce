using System;
using System.Threading;
using RequestReduce.Configuration;
using RequestReduce.Utilities;

namespace RequestReduce.Store
{
    public class DbDiskCache : LocalDiskStore
    {
        protected Timer timer = null;

        public DbDiskCache(IFileWrapper fileWrapper, IRRConfiguration configuration, IUriBuilder uriBuilder) : base(fileWrapper, configuration, uriBuilder)
        {
            fileWrapper.DeleteDirectory(configuration.SpritePhysicalPath);
            const int interval = 1000*60*5;
            timer = new Timer(PurgeOldFiles, null, interval, interval);
        }

        protected virtual void PurgeOldFiles(object state)
        {
            //delete files older than 10 minutes and remove from check list
            throw new NotImplementedException();
        }

        public override void Save(byte[] content, string url, string originalUrls)
        {
            base.Save(content, url, originalUrls);
            //add url to to check list
        }

        public override bool SendContent(string url, System.Web.HttpResponseBase response)
        {
            //if url is not in checklist, return false
            return base.SendContent(url, response);
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
