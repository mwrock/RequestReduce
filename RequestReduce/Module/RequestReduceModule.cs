using System;
using System.Web;
using RequestReduce.Api;
using RequestReduce.Configuration;
using RequestReduce.Handlers;
using RequestReduce.IOC;

namespace RequestReduce.Module
{
    public class RequestReduceModule : IHttpModule
    {
        public void Dispose()
        {
        }

        public void Init(HttpApplication context)
        {
            context.ReleaseRequestState += (sender, e) => InstallFilter(new HttpContextWrapper(((HttpApplication)sender).Context));
            context.PreSendRequestHeaders += (sender, e) => InstallFilter(new HttpContextWrapper(((HttpApplication)sender).Context));
            context.BeginRequest += (sender, e) => HandleRRContent(new HttpContextWrapper(((HttpApplication)sender).Context));
            context.PostAuthenticateRequest += (sender, e) => HandleRRContent(new HttpContextWrapper(((HttpApplication)sender).Context));
        }

        public void HandleRRContent(HttpContextBase httpContextWrapper)
        {
            var handlerFactory = RRContainer.Current.GetInstance<IHandlerFactory>();
            var handler = handlerFactory.ResolveHandler(httpContextWrapper.Request.Url);
            if(handler != null)
            {
                if (HttpContext.Current != null)
                    HttpContext.Current.RemapHandler(handler); //can't use RemapHandler on HttpContextBase due to .net3.5 compat
                else //unit testing
                    httpContextWrapper.Items["remapped handler"] = handler;
            }
        }

        private static bool IsInRRContentDirectory(Uri uri)
        {
            var config = RRContainer.Current.GetInstance<IRRConfiguration>();
            var rrPath = EnsurePath(config.ResourceVirtualPath);
            var url = uri.AbsolutePath;
            if(rrPath.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                url = uri.AbsoluteUri;
            return url.StartsWith(rrPath, StringComparison.OrdinalIgnoreCase);
        }

        public void InstallFilter(HttpContextBase context)
        {
            RRTracer.Trace("Entering Module");
            var uri = context.Request.Url;
            if ((context.Response.ContentType != "text/html" &&
                context.Response.ContentType != "application/xhtml+xml") ||
                uri.AbsolutePath == "/favicon.ico" || 
                IsInRRContentDirectory(uri))
                return;

            ResponseFilter.InstallFilter(context);
        }

        [Obsolete("Use RequestReduce.Api.Registry.CaptureErrorAction")]
        public static Action<Exception> CaptureErrorAction
        {
            set { Registry.CaptureErrorAction = value; }
            get { return Registry.CaptureErrorAction;  }
        }

        private static string EnsurePath(string path)
        {
            if (path.EndsWith("/"))
                return path;
            return path + "/";
        }
    }
}
