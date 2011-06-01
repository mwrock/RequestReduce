using System;
using System.Collections.Generic;
using System.IO;
using RequestReduce.Configuration;
using RequestReduce.Utilities;

namespace RequestReduce.Store
{
    public class LocalDiskStore : IStore
    {
        private readonly IFileWrapper fileWrapper;
        private readonly IRRConfiguration configuration;
        private readonly IUriBuilder uriBuilder;

        public LocalDiskStore(IFileWrapper fileWrapper, IRRConfiguration configuration, IUriBuilder uriBuilder)
        {
            this.fileWrapper = fileWrapper;
            this.configuration = configuration;
            this.uriBuilder = uriBuilder;
        }

        public void Save(byte[] content, string url)
        {
            fileWrapper.Save(content, GetFileNameFromConfig(url));
        }

        public byte[] GetContent(string url)
        {
            throw new NotImplementedException();
        }

        public Stream OpenStream(string url)
        {
            return fileWrapper.OpenStream(GetFileNameFromConfig(url));
        }

        public IDictionary<Guid, string> GetSavedUrls()
        {
            var dic = new Dictionary<Guid, string>();
            if (string.IsNullOrEmpty(configuration.SpritePhysicalPath))
                return dic;
            var keys = fileWrapper.GetDirectories(configuration.SpritePhysicalPath);
            foreach (var key in keys)
            {
                Guid guid;
                var success = Guid.TryParse(key.Substring(key.LastIndexOf('\\') + 1), out guid);
                if(success)
                    dic.Add(guid, uriBuilder.BuildCssUrl(guid));
            }
            return dic;
        }

        private string GetFileNameFromConfig(string url)
        {
            var fileName =  url.Replace(configuration.SpriteVirtualPath, configuration.SpritePhysicalPath).Replace('/', '\\');
            var dir = fileName.Substring(0, fileName.LastIndexOf('\\'));
            if(!fileWrapper.DirectoryExists(dir))
                fileWrapper.CreateDirectory(dir);
            return fileName;
        }
    }
}
