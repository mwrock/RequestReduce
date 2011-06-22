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
        public const string CssFileName = "RequestReducedStyle.css";

        public UriBuilder(IRRConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public string BuildCssUrl(Guid key)
        {
            return string.Format("{0}{1}/{2}-{3}", configuration.ContentHost, configuration.SpriteVirtualPath, key, CssFileName);
        }

        public string BuildSpriteUrl(Guid key, int spriteIndex)
        {
            return string.Format("{0}{1}/{2}-sprite{3}.png", configuration.ContentHost, configuration.SpriteVirtualPath, key, spriteIndex);
        }

        public string ParseFileName(string url)
        {
            return url.Substring(url.LastIndexOf('-') + 1);
        }

        public Guid ParseKey(string url)
        {
            var idx = url.LastIndexOf('-');
            string keyDir = string.Empty;
            if(idx > -1)
            {
                var dir = url.Substring(0, idx);
                keyDir = dir.Substring(dir.LastIndexOf('/') + 1);
            }
            Guid key = Guid.Empty;
            Guid.TryParse(keyDir, out key);
            return key;
        }
    }
}
