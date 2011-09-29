using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Collections.Generic;
using RequestReduce.ResourceTypes;

namespace RequestReduce.Utilities
{
    public interface IWebClientWrapper
    {
        string DownloadString<T>(string url) where T : IResourceType, new();
        string DownloadString(string url);
        byte[] DownloadBytes(string url);
    }

    public class WebClientWrapper : IWebClientWrapper
    {
        public string DownloadString<T>(string url) where T : IResourceType, new()
        {
            return DownloadString(url, new T().SupportedMimeTypes);
        }

        public string DownloadString(string url)
        {
            return DownloadString(url, Enumerable.Empty<string>());
        }

        private string DownloadString(string url, IEnumerable<string> requiredMimeTypes)
        {
            try
            {
                var client = WebRequest.Create(url);
                var systemWebProxy = WebRequest.GetSystemWebProxy();
                systemWebProxy.Credentials = CredentialCache.DefaultCredentials;
                client.Proxy = systemWebProxy;
                using (var response = client.GetResponse())
                {
                    if (requiredMimeTypes.Any() && !requiredMimeTypes.Any(x => response.ContentType.ToLowerInvariant().Contains(x.ToLowerInvariant())))
                        throw new InvalidOperationException(string.Format(
                            "RequestReduce expected url '{0}' to have a mime type of '{1}'.", url, string.Join(" or ", requiredMimeTypes)));
                    using (var responseStream = response.GetResponseStream())
                    {
                        if (responseStream == null)
                            return string.Empty;
                        using (var streameader = new StreamReader(responseStream, Encoding.UTF8))
                        {
                            return streameader.ReadToEnd();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    string.Format("RequestReduce had problems accessing {0}. Error Message from WebClient is: {1}", url,
                                  ex.Message), ex);
            }
        }

        public byte[] DownloadBytes(string url)
        {
            try
            {
                using (var client = new WebClient())
                {
                    var systemWebProxy = WebRequest.GetSystemWebProxy();
                    systemWebProxy.Credentials = CredentialCache.DefaultCredentials;
                    client.Proxy = systemWebProxy;
                    return client.DownloadData(url);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    string.Format("RequestReduce had problems accessing {0}. Error Message from WebClient is: {1}", url,
                                  ex.Message), ex);
            }
        }

    }
}
