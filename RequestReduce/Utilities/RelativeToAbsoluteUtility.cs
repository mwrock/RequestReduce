using System;

namespace RequestReduce.Utilities
{
    public static class RelativeToAbsoluteUtility
    {
        public static string ToAbsolute(Uri baseUrl, string relativeUrl)
        {
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
