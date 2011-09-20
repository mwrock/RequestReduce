using System;
using System.Web;

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
            return IsAbsolute(relativeUrl) ? relativeUrl : new Uri(new Uri(baseUrl), relativeUrl).AbsoluteUri;
        }

        private static bool IsAbsolute(string url)
        {
            return url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                   url.StartsWith("https://", StringComparison.OrdinalIgnoreCase);
        }
    }
}
