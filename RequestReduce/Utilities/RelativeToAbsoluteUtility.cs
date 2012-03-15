using System;
using System.Web;
using RequestReduce.Api;
using RequestReduce.Configuration;

namespace RequestReduce.Utilities
{
    public interface IRelativeToAbsoluteUtility
    {
        string ToAbsolute(Uri baseUrl, string relativeUrl);
        string ToAbsolute(string baseUrl, string relativeUrl);
        string ToAbsolute(string baseUrl, string relativeUrl, bool useContentHost);
    }

    public class RelativeToAbsoluteUtility : IRelativeToAbsoluteUtility
    {
        private readonly HttpContextBase context;
        private readonly IRRConfiguration config;

        public RelativeToAbsoluteUtility(HttpContextBase context, IRRConfiguration config)
        {
            this.context = context;
            this.config = config;
        }

        public string ToAbsolute(Uri baseUrl, string relativeUrl)
        {
            if (context.Request.Headers != null)
            {
                if (context.Request.Headers["X-Forwarded-For"] != null && !baseUrl.IsDefaultPort)
                    baseUrl = new System.UriBuilder(baseUrl.Scheme, baseUrl.Host) { Path = baseUrl.PathAndQuery }.Uri;
            }
            return IsAbsolute(relativeUrl) ? relativeUrl : new Uri(baseUrl, relativeUrl).AbsoluteUri;
        }

        public string ToAbsolute(string baseUrl, string relativeUrl)
        {
            return ToAbsolute(baseUrl, relativeUrl, true);
        }

        private string ReplaceContentHost(string url, string baseUrl)
        {
            var contentHost = config.ContentHost;
            if (string.IsNullOrEmpty(contentHost) && Registry.UrlTransformer != null)
                return Registry.UrlTransformer(context,url, url);
            if (string.IsNullOrEmpty(contentHost))
#pragma warning disable 618
                return Registry.AbsoluteUrlTransformer != null ? Registry.AbsoluteUrlTransformer(url, url) : url;
#pragma warning restore 618
            var urlHost = GetHost(url);
            var baseHost = GetHost(baseUrl);
            var transformedUrl = contentHost + url.Substring(urlHost.Length);
            if (!baseHost.Equals(urlHost, StringComparison.OrdinalIgnoreCase))
                transformedUrl = url;
            if (Registry.UrlTransformer != null)
                return Registry.UrlTransformer(context, url, transformedUrl);
#pragma warning disable 618
            return Registry.AbsoluteUrlTransformer != null
                       ? Registry.AbsoluteUrlTransformer(url, transformedUrl)
                       : transformedUrl;
#pragma warning restore 618
        }

        private static string GetHost(string url)
        {
            var firstPos = url.IndexOf("//", StringComparison.Ordinal);
            if (firstPos > -1)
                firstPos += 2;
            var idx = url.IndexOf('/', firstPos);
            return url.Substring(0, idx);
        }

        private static bool IsAbsolute(string url)
        {
            return url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                   url.StartsWith("https://", StringComparison.OrdinalIgnoreCase);
        }

        public string ToAbsolute(string baseUrl, string relativeUrl, bool useContentHost)
        {
            var absolute = IsAbsolute(relativeUrl) ? relativeUrl : new Uri(new Uri(baseUrl), relativeUrl).AbsoluteUri;
            return useContentHost ? ReplaceContentHost(absolute, baseUrl) : absolute;
        }
    }
}
