using System;
using RequestReduce.Configuration;

namespace RequestReduce.Utilities
{
    public interface IUriBuilder
    {
        string BuildCssUrl(Guid key);
        string BuildSpriteUrl(Guid key, int spriteIndex);
        string ParseFileName(string url);
        Guid ParseKey(string url);
    }

    public class UriBuilder : IUriBuilder
    {
        private readonly IRRConfiguration configuration;

        public UriBuilder(IRRConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public string BuildCssUrl(Guid key)
        {
            return string.Format("{0}{1}/{2}/RequestReducedStyle.css", configuration.ContentHost, configuration.SpriteVirtualPath, key);
        }

        public string BuildSpriteUrl(Guid key, int spriteIndex)
        {
            return string.Format("{0}{1}/{2}/sprite{3}.png", configuration.ContentHost, configuration.SpriteVirtualPath, key, spriteIndex);
        }

        public string ParseFileName(string url)
        {
            return null;
        }

        public Guid ParseKey(string url)
        {
            return Guid.Empty;
        }
    }
}
