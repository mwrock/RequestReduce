using System;
using System.Web;
using RequestReduce.Configuration;
using RequestReduce.IOC;
using RequestReduce.Utilities;

namespace RequestReduce.SampleWeb
{
    public class Demo : IHttpHandler
    {
        public bool IsReusable
        {
            get { return true; }
        }

        public void ProcessRequest(HttpContext context)
        {
            var url = context.Request.RawUrl.Substring(context.Request.RawUrl.IndexOf("test=") + 5);
            var response = new WebClientWrapper().DownloadString(url);
            var config = RRContainer.Current.GetInstance<IRRConfiguration>();
            config.BaseAddress = url;
            context.Response.Write(response);
        }
    }
}
