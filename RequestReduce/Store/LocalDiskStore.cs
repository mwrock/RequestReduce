using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using RequestReduce.Configuration;
using RequestReduce.Utilities;

namespace RequestReduce.Store
{
    public class LocalDiskStore : IStore
    {
        protected readonly IFileWrapper fileWrapper;
        private readonly IRRConfiguration configuration;
        private readonly IUriBuilder uriBuilder;
        private FileSystemWatcher watcher = new FileSystemWatcher();

        public LocalDiskStore(IFileWrapper fileWrapper, IRRConfiguration configuration, IUriBuilder uriBuilder)
        {
            this.fileWrapper = fileWrapper;
            this.configuration = configuration;
            configuration.PhysicalPathChange += SetupWatcher;
            this.uriBuilder = uriBuilder;
            SetupWatcher();
        }

        protected LocalDiskStore()
        {
        }

        protected virtual void SetupWatcher()
        {
            if (string.IsNullOrEmpty(configuration.SpritePhysicalPath)) return;
            if (configuration != null)
            {
                RRTracer.Trace("Setting up File System Watcher for {0}", configuration.SpritePhysicalPath);
                watcher.Path = configuration.SpritePhysicalPath;
            }
            watcher.IncludeSubdirectories = true;
            watcher.Filter = "*.css";
            watcher.Created += new FileSystemEventHandler(OnChange);
            watcher.Deleted += new FileSystemEventHandler(OnChange);
            watcher.Changed += new FileSystemEventHandler(OnChange);
            watcher.EnableRaisingEvents = true;
        }

        private void OnChange(object sender, FileSystemEventArgs e)
        {
            var path = e.FullPath;
            RRTracer.Trace("watcher watched {0}", path);
            var guid = uriBuilder.ParseKey(path.Replace('\\', '/'));
            if(guid != Guid.Empty)
            {
                RRTracer.Trace("New Content {0} and watched: {1}", e.ChangeType, path);
                if (e.ChangeType == WatcherChangeTypes.Deleted && CssDeleted != null)
                    CssDeleted(guid);
                if ((e.ChangeType == WatcherChangeTypes.Created || e.ChangeType == WatcherChangeTypes.Changed) && CssAded != null)
                    CssAded(guid, uriBuilder.BuildCssUrl(guid));
            }
        }

        public virtual void Save(byte[] content, string url, string originalUrls)
        {
            fileWrapper.Save(content, GetFileNameFromConfig(url));
            RRTracer.Trace("{0} saved to disk.", url);
        }

        public virtual bool SendContent(string url, HttpResponseBase response)
        {
            try
            {
                response.TransmitFile(GetFileNameFromConfig(url));
                RRTracer.Trace("{0} transmitted from disk.", url);
                return true;
            }
            catch (FileNotFoundException)
            {
                return false;
            }
        }

        public IDictionary<Guid, string> GetSavedUrls()
        {
            RRTracer.Trace("LocalDiskStore Looking for previously saved content.");
            var dic = new Dictionary<Guid, string>();
            if (configuration == null || string.IsNullOrEmpty(configuration.SpritePhysicalPath))
                return dic;
            var keys = fileWrapper.GetFiles(configuration.SpritePhysicalPath).Select(x => uriBuilder.ParseKey(x)).Distinct();
            foreach (var key in keys)
            {
                if(key != Guid.Empty)
                    dic.Add(key, uriBuilder.BuildCssUrl(key));
            }
            return dic;
        }

        public virtual event DeleeCsAction CssDeleted;
        public virtual event AddCssAction CssAded;
        public void Flush(Guid keyGuid)
        {
            if(CssDeleted != null) CssDeleted(keyGuid);
            var files =
                fileWrapper.GetFiles(configuration.SpritePhysicalPath).Where(
                    x => x.Contains(keyGuid.ToString()) && !x.Contains("Expired"));
            foreach (var file in files)
                fileWrapper.RenameFile(file, file.Replace(keyGuid.ToString(), keyGuid + "-Expired"));
        }

        protected virtual string GetFileNameFromConfig(string url)
        {
            var fileName = url;
            if (!string.IsNullOrEmpty(configuration.ContentHost))
                fileName = url.Replace(configuration.ContentHost, "");
            return fileName.Replace(configuration.SpriteVirtualPath, configuration.SpritePhysicalPath).Replace('/', '\\');
        }

        public virtual void Dispose()
        {
            watcher.EnableRaisingEvents = false;
            watcher.Dispose();
            RRTracer.Trace("Local Disk Store Disposed.");
        }
    }
}
