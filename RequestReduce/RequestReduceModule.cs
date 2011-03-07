using System;
using System.IO;
using System.Web;
using RequestReduce.Filter;

namespace RequestReduce
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
                var response = context.Response;

                if (response.ContentType == "text/html")
                    response.Filter = RRContainer.Current.GetInstance<AbstractFilter>();

                context.Items.Add(CONTEXT_KEY, new object());
            }
        }
    }
}
