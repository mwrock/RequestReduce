using System;
using System.Linq;
using System.Web;
using RequestReduce.Configuration;
using RequestReduce.Store;
using RequestReduce.Utilities;

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
            context.PostAuthenticateRequest += (sender, e) => HandleRRFlush(new HttpContextWrapper(((HttpApplication)sender).Context));
        }

        public void HandleRRFlush(HttpContextBase httpContextWrapper)
        {
            var url = httpContextWrapper.Request.RawUrl;
            if (!IsInRRContentDirectory(httpContextWrapper) ||
                !url.EndsWith("/flush", StringComparison.OrdinalIgnoreCase)) return;

            var config = RRContainer.Current.GetInstance<IRRConfiguration>();
            var user = httpContextWrapper.User.Identity.Name;
            if (config.AuthorizedUserList.AllowsAnonymous() || config.AuthorizedUserList.Contains(user))
            {
                var store = RRContainer.Current.GetInstance<IStore>();
                var uriBuilder = RRContainer.Current.GetInstance<IUriBuilder>();
                var key = uriBuilder.ParseKey(url.ToLower().Replace("/flush", "-flush"));
                store.Flush(key);
                RRTracer.Trace("{0} Flushed {1}", user, key);
            }
            if (httpContextWrapper.ApplicationInstance != null)
                httpContextWrapper.ApplicationInstance.CompleteRequest();
        }

        public void HandleRRContent(HttpContextBase httpContextWrapper)
        {
            var url = httpContextWrapper.Request.RawUrl;
            if (!IsInRRContentDirectory(httpContextWrapper) || url.EndsWith("/flush", StringComparison.OrdinalIgnoreCase)) return;

            RRTracer.Trace("Beginning to serve {0}", url);
            var store = RRContainer.Current.GetInstance<IStore>();
            if (store.SendContent(url, httpContextWrapper.Response))
            {
                httpContextWrapper.Response.Headers.Remove("ETag");
                httpContextWrapper.Response.Cache.SetCacheability(HttpCacheability.Public);
                httpContextWrapper.Response.Expires = 44000;
                if (url.EndsWith(".css", StringComparison.OrdinalIgnoreCase))
                    httpContextWrapper.Response.ContentType = "text/css";
                else if (url.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                    httpContextWrapper.Response.ContentType = "image/png";
                if (httpContextWrapper.ApplicationInstance != null)
                    httpContextWrapper.ApplicationInstance.CompleteRequest();
            }
            RRTracer.Trace("Finished serving {0}", url);
        }

        private static bool IsInRRContentDirectory(HttpContextBase httpContextWrapper)
        {
            var config = RRContainer.Current.GetInstance<IRRConfiguration>();
            var rrPath = config.SpriteVirtualPath;
            var url = httpContextWrapper.Request.RawUrl;
            if(rrPath.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                url = httpContextWrapper.Request.Url.AbsoluteUri;
            return url.StartsWith(rrPath, StringComparison.OrdinalIgnoreCase);
        }

        public void InstallFilter(HttpContextBase context)
        {
            var request = context.Request;
            if (context.Items.Contains(CONTEXT_KEY) || 
                context.Response.ContentType != "text/html" || 
                request.QueryString["RRFilter"] == "disabled" || 
                request.RawUrl == "/favicon.ico" || 
                IsInRRContentDirectory(context))
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
