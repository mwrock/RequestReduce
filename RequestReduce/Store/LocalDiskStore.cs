using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using RequestReduce.Configuration;
using RequestReduce.Module;
using RequestReduce.Utilities;
using UriBuilder = RequestReduce.Utilities.UriBuilder;
using RequestReduce.Reducer;
using RequestReduce.IOC;
using RequestReduce.ResourceTypes;

namespace RequestReduce.Store
{
    public class LocalDiskStore : IStore
    {
        protected readonly IFileWrapper fileWrapper;
        private readonly IRRConfiguration configuration;
        private readonly IUriBuilder uriBuilder;
        protected IReductionRepository reductionRepository;
        private FileSystemWatcher watcher = null;

        public LocalDiskStore(IFileWrapper fileWrapper, IRRConfiguration configuration, IUriBuilder uriBuilder, IReductionRepository reductionRepository)
        {
            this.fileWrapper = fileWrapper;
            this.configuration = configuration;
            configuration.PhysicalPathChange += SetupWatcher;
            this.uriBuilder = uriBuilder;
            this.reductionRepository = reductionRepository;
            if(configuration.IsFullTrust)
                SetupWatcher();
        }

        protected LocalDiskStore()
        {
        }

        protected virtual void SetupWatcher()
        {
            if (string.IsNullOrEmpty(configuration.SpritePhysicalPath)) return;
            watcher = new FileSystemWatcher();
            if (configuration != null)
            {
                RRTracer.Trace("Setting up File System Watcher for {0}", configuration.SpritePhysicalPath);
                watcher.Path = configuration.SpritePhysicalPath;
            }
            watcher.IncludeSubdirectories = true;
            watcher.Filter = "*RequestReduce*";
            watcher.Created += OnChange;
            watcher.Deleted += OnChange;
            watcher.Changed += OnChange;
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
                var resourceType = RRContainer.Current.GetAllInstances<IResourceType>().SingleOrDefault(x => path.EndsWith(x.FileName));
                if (resourceType != null)
                {
                    RRTracer.Trace("New Content {0} and watched: {1}", e.ChangeType, path);
                    if (e.ChangeType == WatcherChangeTypes.Deleted)
                        reductionRepository.RemoveReduction(guid);
                    if ((e.ChangeType == WatcherChangeTypes.Created || e.ChangeType == WatcherChangeTypes.Changed))
                        reductionRepository.AddReduction(guid, uriBuilder.BuildResourceUrl(guid, contentSignature, resourceType.GetType()));
                }
            }
        }

        public virtual void Save(byte[] content, string url, string originalUrls)
        {
            var file = GetFileNameFromConfig(url);
            var sig = uriBuilder.ParseSignature(url);
            var guid = uriBuilder.ParseKey(url);
            fileWrapper.Save(content, file);
            if (!url.ToLower().EndsWith(".png") && reductionRepository != null)
                reductionRepository.AddReduction(guid, url);
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

            var activeFiles = fileWrapper.GetDatedFiles(configuration.SpritePhysicalPath, "*RequestReduce*");
            return (from files in activeFiles 
			where !files.FileName.Contains("-Expired-") 
			group files by uriBuilder.ParseKey(files.FileName.Replace("\\", "/")) into filegroup 
			join files2 in activeFiles on new { k = filegroup.Key, u = filegroup.Max(m => m.CreatedDate) } equals new { k = uriBuilder.ParseKey(files2.FileName.Replace("\\", "/")), u = files2.CreatedDate } select files2.FileName)
            .ToDictionary(file => uriBuilder.ParseKey(file.Replace("\\", "/")), file => uriBuilder.BuildResourceUrl(uriBuilder.ParseKey(file.Replace("\\", "/")), uriBuilder.ParseSignature(file.Replace("\\", "/")), RRContainer.Current.GetAllInstances<IResourceType>().SingleOrDefault(x => file.EndsWith(x.FileName)).GetType()));
        }

        public void Flush(Guid keyGuid)
        {
            if (keyGuid == Guid.Empty)
            {
                var urls = GetSavedUrls();
                foreach (var key in urls.Keys)
                    Flush(key);
            }

            reductionRepository.RemoveReduction(keyGuid); 
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


        public string GetUrlByKey(Guid keyGuid, Type resourceType)
        {
            return null;
        }
    }
}
