using System.Drawing;
using System.IO;
using System.Net;

namespace RequestReduce.Utilities
{
    public interface IWebClientWrapper
    {
        string DownloadString(string url);
        Bitmap DownloadImage(string imageUrl);
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

        public Bitmap DownloadImage(string url)
        {
            using(var client = new WebClient())
            {
                var bytes = client.DownloadData(url);
                return new Bitmap(new MemoryStream(bytes));
            }
        }

    }
}
