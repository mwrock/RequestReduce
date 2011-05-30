using System;
using System.IO;
using RequestReduce.Configuration;
using RequestReduce.Utilities;

namespace RequestReduce.Store
{
    public class LocalDiskStore : IStore
    {
        private readonly IFileWrapper fileWrapper;
        private readonly IRRConfiguration configuration;

        public LocalDiskStore(IFileWrapper fileWrapper, IRRConfiguration configuration)
        {
            this.fileWrapper = fileWrapper;
            this.configuration = configuration;
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
