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
            using(var client = new WebClient())
            {
                return client.DownloadString(url);
            }
        }

        public byte[] DownloadBytes(string url)
        {
            using(var client = new WebClient())
            {
                return client.DownloadData(url);
            }
        }

    }
}
