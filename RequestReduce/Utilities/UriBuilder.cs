using System;
using System.Web;
using RequestReduce.Api;
using RequestReduce.Configuration;
using RequestReduce.ResourceTypes;
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
            return BuildResourceUrl<T>(key, bytes.Length > 0 ? Hasher.Hash(bytes).RemoveDashes() : Guid.Empty.RemoveDashes());
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
            var url = string.Format("{0}{1}/{2}-{3}-{4}", configuration.ContentHost, configuration.ResourceAbsolutePath, key.RemoveDashes(), signature, resource.FileName);
            return Registry.UrlTransformer != null
                                   ? Registry.UrlTransformer(RRContainer.Current.GetInstance<HttpContextBase>(), null, url)
                                   : url;
        }

        public string BuildSpriteUrl(Guid key, byte[] bytes)
        {
            var url = string.Format("{0}{1}/{2}-{3}.png", configuration.ContentHost, configuration.ResourceAbsolutePath, key.RemoveDashes(), Hasher.Hash(bytes).RemoveDashes());
            return Registry.UrlTransformer != null
                                   ? Registry.UrlTransformer(RRContainer.Current.GetInstance<HttpContextBase>(), null, url)
                                   : url;
        }

        public string ParseFileName(string url)
        {
            if (string.IsNullOrEmpty(url))
                return null;

            return string.IsNullOrEmpty(url.Trim()) ? null : url.Substring(url.LastIndexOf('/') + 1);
        }

        public Guid ParseKey(string url)
        {
            if (string.IsNullOrEmpty(url))
                return Guid.Empty;

            var idx = url.LastIndexOf('/');
            var keyDir = idx > -1 ? url.Substring(idx + 1) : url;
            var strKey = string.Empty;
            idx = keyDir.IndexOf('-');
            if (idx > -1)
                strKey = keyDir.Substring(0, idx);

            try
            {
                return new Guid(strKey);
            }
            catch (FormatException)
            {
                return Guid.Empty;
            }
        }

        public string ParseSignature(string url)
        {
            if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(url.Trim()))
                return Guid.Empty.RemoveDashes();

            var idx = url.LastIndexOf('/');
            var keyDir = idx > -1 ? url.Substring(idx + 1) : url;
            try
            {
                var strKey = keyDir.Substring(33, 32);
                return new Guid(strKey).RemoveDashes();
            }
            catch (Exception)
            {
                return Guid.Empty.RemoveDashes();
            }
        }
    }
}
