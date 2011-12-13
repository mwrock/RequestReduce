using System;
using System.Web;
using RequestReduce.Configuration;
using RequestReduce.IOC;

namespace RequestReduce.Utilities
{
    public static class RelativeToAbsoluteUtility
    {
        public static string ToAbsolute(Uri baseUrl, string relativeUrl)
        {
            var context = HttpContext.Current;
            if (context != null)
            {
                if (context.Request.Headers["X-Forwarded-For"] != null && !baseUrl.IsDefaultPort)
                    baseUrl = new System.UriBuilder(baseUrl.Scheme, baseUrl.Host) { Path = baseUrl.PathAndQuery }.Uri;
            }
            return IsAbsolute(relativeUrl) ? relativeUrl : new Uri(baseUrl, relativeUrl).AbsoluteUri;
        }

        public static string ToAbsolute(string baseUrl, string relativeUrl)
        {
            return ReplaceContentHost(IsAbsolute(relativeUrl) ? relativeUrl : new Uri(new Uri(baseUrl), relativeUrl).AbsoluteUri);
        }

        private static string ReplaceContentHost(string url)
        {
            var contentHost = RRContainer.Current.GetInstance<IRRConfiguration>().ContentHost;
            if (string.IsNullOrEmpty(contentHost))
                return url;
            var firstPos = url.IndexOf("//");
            if (firstPos > -1)
                firstPos += 2;
            var idx = url.IndexOf('/', firstPos);
            return contentHost + url.Substring(idx);
        }

        private static bool IsAbsolute(string url)
        {
            return url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                   url.StartsWith("https://", StringComparison.OrdinalIgnoreCase);
        }
    }
}
