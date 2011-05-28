using System;
using System.Web;
using RequestReduce.Configuration;

namespace RequestReduce.Module
{
    public class RequestReduceModule : IHttpModule
    {
        public readonly string CONTEXT_KEY = "HttpOnlyFilteringModuleInstalled";
        public void Dispose()
        {
        }

        public void Init(HttpApplication context)
        {
            context.ReleaseRequestState += (sender, e) => InstallFilter(new HttpContextWrapper(((HttpApplication)sender).Context));
            context.PreSendRequestHeaders += (sender, e) => InstallFilter(new HttpContextWrapper(((HttpApplication)sender).Context));
        }

        public void InstallFilter(HttpContextBase context)
        {
            if (!context.Items.Contains(CONTEXT_KEY))
            {
                var config = RRContainer.Current.GetInstance<IRRConfiguration>();
                config.SpritePhysicalPath = context.Server.MapPath(config.SpriteVirtualPath);
                var response = context.Response;

                if (response.ContentType == "text/html" && context.Request.QueryString["RRFilter"] != "disabled")
                    response.Filter = RRContainer.Current.GetInstance<AbstractFilter>();

                context.Items.Add(CONTEXT_KEY, new object());
            }
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
