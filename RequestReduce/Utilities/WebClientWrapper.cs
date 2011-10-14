using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Collections.Generic;
using RequestReduce.ResourceTypes;
using RequestReduce.IOC;

namespace RequestReduce.Utilities
{
    public interface IWebClientWrapper
    {
        WebResponse Download<T>(string url) where T : IResourceType;
        string DownloadString<T>(string url) where T : IResourceType;
        string DownloadString(string url);
        byte[] DownloadBytes(string url);
    }

    public class WebClientWrapper : IWebClientWrapper
    {
        public WebResponse Download<T>(string url) where T : IResourceType
        {
            return Download(url, RRContainer.Current.GetInstance<T>().SupportedMimeTypes);
        }

        public string DownloadString(string url)
        {
            using (var response = Download(url, Enumerable.Empty<string>()))
            {
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

        private WebResponse Download(string url, IEnumerable<string> requiredMimeTypes)
        {
            try
            {
                var client = WebRequest.Create(url);
                var systemWebProxy = WebRequest.GetSystemWebProxy();
                systemWebProxy.Credentials = CredentialCache.DefaultCredentials;
                client.Proxy = systemWebProxy;
                var response = client.GetResponse();
                if (response.ContentLength > 0 && requiredMimeTypes.Any() && !requiredMimeTypes.Any(x => response.ContentType.ToLowerInvariant().Contains(x.ToLowerInvariant())))
                    throw new InvalidOperationException(string.Format(
                        "RequestReduce expected url '{0}' to have a mime type of '{1}'.", url, string.Join(" or ", requiredMimeTypes.ToArray())));
                return response;
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

        public string DownloadString<T>(string url) where T : IResourceType
        {
            string cssContent = string.Empty;
            using (var response = Download<T>(url))
            {
                if (response == null)
                    return null;
                using (var responseStream = response.GetResponseStream())
                {
                    if (responseStream != null)
                    {
                        using (var streameader = new StreamReader(responseStream, Encoding.UTF8))
                        {
                            cssContent = streameader.ReadToEnd();
                        }
                    }
                }
            }

            return cssContent;
        }
    }
}
