using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Collections.Generic;

namespace RequestReduce.Utilities
{
    public interface IWebClientWrapper
    {
        string DownloadCssString(string url);
        string DownloadJavaScriptString(string url);
        byte[] DownloadBytes(string url);
    }

    public class WebClientWrapper : IWebClientWrapper
    {
        public string DownloadJavaScriptString(string url)
        {
            return DownloadString(url, new []{ "text/javascript", "application/javascript", "application/x-javascript" });
        }

        public string DownloadCssString(string url)
        {
            return DownloadString(url, new[]{"text/css"});
        }

        private string DownloadString(string url, IEnumerable<string> requiredMimeTypes)
        {
            try
            {
                var client = WebRequest.Create(url);
                using (var response = client.GetResponse())
                {
                    if (!requiredMimeTypes.Any(x => x.Equals(response.ContentType, StringComparison.OrdinalIgnoreCase)))
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
