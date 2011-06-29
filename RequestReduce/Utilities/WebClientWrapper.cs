using System;
using System.Drawing;
using System.IO;
using System.Net;

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
                using (var client = new WebClient())
                {
                    return client.DownloadString(url);
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
