using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using RequestReduce.Configuration;
using RequestReduce.Utilities;
using UriBuilder = RequestReduce.Utilities.UriBuilder;

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
            var contentSignature = uriBuilder.ParseSignature(path.Replace('\\', '/'));
            if(guid != Guid.Empty)
            {
                RRTracer.Trace("New Content {0} and watched: {1}", e.ChangeType, path);
                if (e.ChangeType == WatcherChangeTypes.Deleted && CssDeleted != null)
                    CssDeleted(guid);
                if ((e.ChangeType == WatcherChangeTypes.Created || e.ChangeType == WatcherChangeTypes.Changed) && CssAded != null)
                    CssAded(guid, uriBuilder.BuildCssUrl(guid, contentSignature));
            }
        }

        public virtual void Save(byte[] content, string url, string originalUrls)
        {
            var file = GetFileNameFromConfig(url);
            var sig = uriBuilder.ParseSignature(url);
            fileWrapper.Save(content, file);
            RRTracer.Trace("{0} saved to disk.", url);
            var expiredFile = file.Insert(file.IndexOf(sig), "Expired-");
            if (fileWrapper.FileExists(expiredFile))
                fileWrapper.DeleteFile(expiredFile);
        }

        public virtual bool SendContent(string url, HttpResponseBase response)
        {
            var file = GetFileNameFromConfig(url);
            try
            {
                response.TransmitFile(file);
                RRTracer.Trace("{0} transmitted from disk.", url);
                return true;
            }
            catch (FileNotFoundException)
            {
                try
                {
                    response.TransmitFile(file.Insert(file.LastIndexOf('-'), "-Expired"));
                    RRTracer.Trace("{0} was expired and transmitted from disk.", url);
                    return true;
                }
                catch (FileNotFoundException)
                {
                    return false;
                }
            }
        }

        public IDictionary<Guid, string> GetSavedUrls()
        {
            RRTracer.Trace("LocalDiskStore Looking for previously saved content.");
            var dic = new Dictionary<Guid, string>();
            if (configuration == null || string.IsNullOrEmpty(configuration.SpritePhysicalPath))
                return dic;
            var files =
                fileWrapper.GetFiles(configuration.SpritePhysicalPath).Where(y => !y.Contains("-Expired-") && y.EndsWith(UriBuilder.CssFileName));
            foreach (var file in files)
            {
                var urlFile = file.Replace("\\", "/");
                dic.Add(uriBuilder.ParseKey(urlFile), uriBuilder.BuildCssUrl(uriBuilder.ParseKey(urlFile), uriBuilder.ParseSignature(urlFile)));
            }
            return dic;
        }

        public virtual event DeleeCsAction CssDeleted;
        public virtual event AddCssAction CssAded;
        public void Flush(Guid keyGuid)
        {
            if (keyGuid == Guid.Empty)
            {
                var urls = GetSavedUrls();
                foreach (var key in urls.Keys)
                    Flush(key);
            }

            if(CssDeleted != null) CssDeleted(keyGuid);
            var files =
                fileWrapper.GetFiles(configuration.SpritePhysicalPath).Where(
                    x => x.Contains(keyGuid.RemoveDashes()) && !x.Contains("Expired"));
            foreach (var file in files)
                fileWrapper.RenameFile(file, file.Replace(keyGuid.RemoveDashes(), keyGuid.RemoveDashes() + "-Expired"));
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
