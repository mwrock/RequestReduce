using System;
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
                using (var client = new WebClient())
                {
                    var remove = 0;
                    var bytes = client.DownloadData(url);
                    if (bytes.Length >= 3 && bytes.Take(3).SequenceEqual(Encoding.UTF8.GetPreamble()))
                        remove = 3;
                    var str = Encoding.UTF8.GetString(bytes, remove, bytes.Length-remove);
                    return str;
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
