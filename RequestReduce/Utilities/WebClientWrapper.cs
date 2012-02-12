using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Collections.Generic;
using RequestReduce.Configuration;
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
        private readonly IWebProxy proxy;

        public WebClientWrapper()
        {
            if (!RRContainer.Current.GetInstance<RRConfiguration>().IsFullTrust && Environment.Version.Major < 4)
                return;
            proxy = WebRequest.GetSystemWebProxy();
            proxy.Credentials = CredentialCache.DefaultCredentials;
        }

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
                client.Credentials = CredentialCache.DefaultCredentials;
                if (RRContainer.Current.GetInstance<RRConfiguration>().IsFullTrust || Environment.Version.Major >= 4) 
                    client.Proxy = proxy;
                var response = client.GetResponse();
                var hasMimeRestrictions = false;
                var meetsMimeRestrictions = false;
                var mimeRestrictionString = new StringBuilder();
                foreach (var requiredMimeType in requiredMimeTypes)
                {
                    hasMimeRestrictions = true;
                    if (response.ContentType.ToLowerInvariant().Contains(requiredMimeType.ToLowerInvariant()))
                    {
                        meetsMimeRestrictions = true;
                        break;
                    }
                    if (mimeRestrictionString.Length > 0)
                        mimeRestrictionString.Append(" or ");
                    mimeRestrictionString.Append(requiredMimeType);
                }
                if (response.ContentLength > 0 && hasMimeRestrictions && !meetsMimeRestrictions)
                    throw new InvalidOperationException(string.Format(
                        "RequestReduce expected url '{0}' to have a mime type of '{1}'.", url, mimeRestrictionString));
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
                    if (RRContainer.Current.GetInstance<RRConfiguration>().IsFullTrust || Environment.Version.Major >= 4)
                        client.Proxy = proxy;
                    client.Credentials = CredentialCache.DefaultCredentials;
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
