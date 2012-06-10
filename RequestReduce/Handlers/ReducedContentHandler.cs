using System;
using System.Web;
using RequestReduce.Configuration;
using RequestReduce.Store;
using RequestReduce.Utilities;

namespace RequestReduce.Handlers
{
    public class ReducedContentHandler : IHttpHandler
    {
        private readonly IRRConfiguration config;
        private readonly IHostingEnvironmentWrapper hostingEnvironment;
        private readonly IUriBuilder uriBuilder;
        private readonly IStore store;

        public ReducedContentHandler(IRRConfiguration config, IHostingEnvironmentWrapper hostingEnvironment, IUriBuilder uriBuilder, IStore store)
        {
            this.config = config;
            this.hostingEnvironment = hostingEnvironment;
            this.uriBuilder = uriBuilder;
            this.store = store;
        }

        public void ProcessRequest(HttpContext context)
        {
            ProcessRequest(new HttpContextWrapper(context));
        }

        public void ProcessRequest(HttpContextBase context)
        {
            var url = context.Request.Url.AbsolutePath;

            if (string.IsNullOrEmpty(config.ResourcePhysicalPath))
                config.ResourcePhysicalPath = hostingEnvironment.MapPath(config.ResourceVirtualPath);

            RRTracer.Trace("Beginning to serve {0}", url);
            var sig = uriBuilder.ParseSignature(url);
            if (sig == Guid.Empty.RemoveDashes())
                return;
            var etag = context.Request.Headers["If-None-Match"];
            etag = etag == null ? string.Empty : etag.Replace("\"", "");
            if (sig == etag || store.SendContent(url, context.Response))
            {
                context.Response.Cache.SetETag(string.Format(@"""{0}""", sig));
                context.Response.Cache.SetCacheability(HttpCacheability.Public);
                context.Response.Expires = 60 * 24 * 360; //LITTLE under A YEAR
                if (sig == etag)
                    context.Response.StatusCode = 304;
                else if (url.EndsWith(".css", StringComparison.OrdinalIgnoreCase))
                    context.Response.ContentType = "text/css";
                else if (url.EndsWith(".js", StringComparison.OrdinalIgnoreCase))
                    context.Response.ContentType = "application/x-javascript";
                else if (url.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                    context.Response.ContentType = "image/png";
                if (context.ApplicationInstance != null)
                    context.ApplicationInstance.CompleteRequest();
            }
            RRTracer.Trace("Finished serving {0}", url);
        }

        public bool IsReusable
        {
            get { return true; }
        }
    }
}
