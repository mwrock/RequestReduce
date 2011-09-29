using System;
using RequestReduce.Configuration;
using RequestReduce.Reducer;
using RequestReduce.ResourceTypes;
using System.Security.AccessControl;
using RequestReduce.IOC;

namespace RequestReduce.Utilities
{
    public interface IUriBuilder
    {
        string BuildResourceUrl<T>(Guid key, byte[] bytes) where T : IResourceType;
        string BuildResourceUrl<T>(Guid key, string signature) where T : IResourceType;
        string BuildResourceUrl(Guid key, byte[] bytes, Type type);
        string BuildResourceUrl(Guid key, string signature, Type resourceType);
        string BuildSpriteUrl(Guid key, byte[] bytes);
        string ParseFileName(string url);
        Guid ParseKey(string url);
        string ParseSignature(string url);
    }

    public class UriBuilder : IUriBuilder
    {
        private readonly IRRConfiguration configuration;

        public UriBuilder(IRRConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public string BuildResourceUrl(Guid key, byte[] bytes, Type type)
        {
            return BuildResourceUrl(key, Hasher.Hash(bytes).RemoveDashes(), type);
        }

        public string BuildResourceUrl<T>(Guid key, byte[] bytes) where T : IResourceType
        {
            return BuildResourceUrl<T>(key, Hasher.Hash(bytes).RemoveDashes());
        }

        public string BuildResourceUrl<T>(Guid key, string signature) where T : IResourceType
        {
            return BuildResourceUrl(key, signature, typeof(T));
        }

        public string BuildResourceUrl(Guid key, string signature, Type resourceType)
        {
            var resource = RRContainer.Current.GetInstance(resourceType) as IResourceType;
            if (resource == null)
                throw new ArgumentException("resourceType must derrive from IResourceType", "resourceType");
            return string.Format("{0}{1}/{2}-{3}-{4}", configuration.ContentHost, configuration.SpriteVirtualPath, key.RemoveDashes(), signature, resource.FileName);
        }

        public string BuildSpriteUrl(Guid key, byte[] bytes)
        {
            return string.Format("{0}{1}/{2}-{3}.png", configuration.ContentHost, configuration.SpriteVirtualPath, key.RemoveDashes(), Hasher.Hash(bytes).RemoveDashes());
        }

        public string ParseFileName(string url)
        {
            return string.IsNullOrWhiteSpace(url) ? null : url.Substring(url.LastIndexOf('/') + 1);
        }

        public Guid ParseKey(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return Guid.Empty;

            var idx = url.LastIndexOf('/');
            string keyDir = idx > -1 ? url.Substring(idx + 1) : url;
            string strKey = string.Empty;
            idx = keyDir.IndexOf('-');
            if (idx > -1)
                strKey = keyDir.Substring(0, idx);
            Guid key;
            Guid.TryParse(strKey, out key);
            return key;
        }

        public string ParseSignature(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return Guid.Empty.RemoveDashes();

            var idx = url.LastIndexOf('/');
            var keyDir = idx > -1 ? url.Substring(idx + 1) : url;
            var strKey = string.Empty;
            try
            {
                strKey = keyDir.Substring(33, 32);
            }
            catch (ArgumentOutOfRangeException)
            {
            }
            Guid key;
            Guid.TryParse(strKey, out key);
            return key.RemoveDashes();
        }
    }
}
