using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace RequestReduce.Utilities
{
    public interface IWebClientWrapper
    {
        string DownloadString(string url);
        byte[] DownloadBytes(string url);
    }

    public class WebClientWrapper : IWebClientWrapper
    {
        public string DownloadString(string url)
        {
            try
            {
                var client = WebRequest.Create(url);
                using (var response = client.GetResponse())
                {
                    if(!response.ContentType.Equals("text/css", StringComparison.OrdinalIgnoreCase))
                        throw new InvalidOperationException(
                            "RequestReduce has landed on a css url that does not contain a css mime type.");
                    using(var responseStream = response.GetResponseStream())
                    {
                        if (responseStream == null)
                            return string.Empty;
                        using(var streameader = new StreamReader(responseStream, Encoding.UTF8))
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
