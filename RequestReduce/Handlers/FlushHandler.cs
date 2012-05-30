using System;
using System.Linq;
using System.Web;
using RequestReduce.Configuration;
using RequestReduce.Module;
using RequestReduce.Store;
using RequestReduce.Utilities;

namespace RequestReduce.Handlers
{
    public class FlushHandler : IHttpHandler
    {
        private readonly IRRConfiguration configuration;
        private readonly IHostingEnvironmentWrapper hostingEnvironment;
        private readonly IIpFilter ipFilter;
        private readonly IReducingQueue queue;
        private readonly IUriBuilder uriBuilder;
        private readonly IStore store;

        public FlushHandler(IRRConfiguration configuration, IHostingEnvironmentWrapper hostingEnvironment, IIpFilter ipFilter, IReducingQueue queue, IUriBuilder uriBuilder, IStore store)
        {
            this.configuration = configuration;
            this.hostingEnvironment = hostingEnvironment;
            this.ipFilter = ipFilter;
            this.queue = queue;
            this.uriBuilder = uriBuilder;
            this.store = store;
        }

        public void ProcessRequest(HttpContext context)
        {
            ProcessRequest(new HttpContextWrapper(context));
        }

        public void ProcessRequest(HttpContextBase context)
        {
            var url = context.Request.RawUrl;
            url = url.EndsWith("/") ? url : url + "/";
            if (string.IsNullOrEmpty(configuration.SpritePhysicalPath))
                configuration.SpritePhysicalPath = hostingEnvironment.MapPath(configuration.SpriteVirtualPath);
            var user = context.User == null ? string.Empty : context.User.Identity.Name;
            if ((configuration.AuthorizedUserList.AllowsAnonymous() || configuration.AuthorizedUserList.Contains(user)) &&
                ipFilter.IsAuthorizedIpAddress(context))
            {
                if (url.ToLowerInvariant().Contains("/flushfailures/"))
                {
                    queue.ClearFailures();
                    RRTracer.Trace("Failures Cleared");
                }
                else
                {
                    var key = uriBuilder.ParseKey(url.ToLower().Replace("/flush/rrflush.aspx/", "-flush"));
                    if (key == Guid.Empty)
                        key = uriBuilder.ParseKey(url.ToLower().Replace("/flush/", "-flush"));
                    store.Flush(key);
                    RRTracer.Trace("{0} Flushed {1}", user, key);
                }
                if (HttpContext.Current != null)
                    context.Response.Redirect(string.Format("{0}/dashboard", configuration.SpriteVirtualPath));
            }
            else
                context.Response.StatusCode = 401;
        }

        public bool IsReusable
        {
            get { return true; }
        }
    }
}
