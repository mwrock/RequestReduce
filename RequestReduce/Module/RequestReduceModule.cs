using System;
using System.Web;
using RequestReduce.Configuration;
using RequestReduce.Store;

namespace RequestReduce.Module
{
    public class RequestReduceModule : IHttpModule
    {
        public const string CONTEXT_KEY = "HttpOnlyFilteringModuleInstalled";
        public void Dispose()
        {
        }

        public void Init(HttpApplication context)
        {
            context.ReleaseRequestState += (sender, e) => InstallFilter(new HttpContextWrapper(((HttpApplication)sender).Context));
            context.PreSendRequestHeaders += (sender, e) => InstallFilter(new HttpContextWrapper(((HttpApplication)sender).Context));
            context.BeginRequest += (sender, e) => HandleRRContent(new HttpContextWrapper(((HttpApplication)sender).Context));
        }

        public void HandleRRContent(HttpContextBase httpContextWrapper)
        {
            if (IsInRRContentDirectory(httpContextWrapper))
            {
                var url = httpContextWrapper.Request.RawUrl;
                RRTracer.Trace("Beginning to serve {0}", url);
                var store = RRContainer.Current.GetInstance<IStore>();
                if(store.SendContent(url, httpContextWrapper.Response))
                {
                    httpContextWrapper.Response.Headers.Remove("ETag");
                    httpContextWrapper.Response.Cache.SetCacheability(HttpCacheability.Public);
                    httpContextWrapper.Response.Expires = 44000;
                    if (httpContextWrapper.ApplicationInstance != null) 
                        httpContextWrapper.ApplicationInstance.CompleteRequest();
                }
                RRTracer.Trace("Finished serving {0}", url);
            }
        }

        private bool IsInRRContentDirectory(HttpContextBase httpContextWrapper)
        {
            var config = RRContainer.Current.GetInstance<IRRConfiguration>();
            var rrPath = config.SpriteVirtualPath.ToLower();
            var url = httpContextWrapper.Request.RawUrl.ToLower();
            if(rrPath.StartsWith("http"))
                url = httpContextWrapper.Request.Url.AbsoluteUri.ToLower();
            return url.StartsWith(rrPath);
        }

        public void InstallFilter(HttpContextBase context)
        {
            var request = context.Request;
            if (context.Items.Contains(CONTEXT_KEY) || context.Response.ContentType != "text/html" || request.QueryString["RRFilter"] == "disabled" || request.RawUrl == "/favicon.ico")
                return;

            var config = RRContainer.Current.GetInstance<IRRConfiguration>();
            if(string.IsNullOrEmpty(config.SpritePhysicalPath))
                config.SpritePhysicalPath = context.Server.MapPath(config.SpriteVirtualPath);
            context.Response.Filter = RRContainer.Current.GetInstance<AbstractFilter>();
            RRTracer.Trace("Attaching Filter to {0}", request.RawUrl);

            context.Items.Add(CONTEXT_KEY, new object());
        }

        public static int QueueCount
        {
            get { return RRContainer.Current.GetInstance<IReducingQueue>().Count; }
        }

        public static void CaptureError(Action<Exception> captureAction)
        {
            RRContainer.Current.GetInstance<IReducingQueue>().CaptureError(captureAction);
        }
    }
}
