using System;
using System.Web;
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
            var config = Api.Registry.Configuration;
            config.BaseAddress = url;
            if (response.IndexOf("<base ", StringComparison.OrdinalIgnoreCase) == -1)
                response = response.Replace("</head>", string.Format(@"<base href=""{0}""></base></head>", url));
            context.Response.Write(response);
        }
    }
}
