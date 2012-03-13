using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using RequestReduce.Api;
using RequestReduce.Configuration;
using RequestReduce.IOC;
using RequestReduce.Store;
using RequestReduce.Utilities;

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
            context.PostAuthenticateRequest += (sender, e) => HandleAuthenticatedActions(new HttpContextWrapper(((HttpApplication)sender).Context));
        }

        private void HandleAuthenticatedActions(HttpContextWrapper httpContextWrapper)
        {
            var url = EnsurePath(httpContextWrapper.Request.RawUrl);
            if (IsInRRContentDirectory(httpContextWrapper) && (
                url.ToLowerInvariant().Contains("/flush/")
                || url.ToLowerInvariant().Contains("/flushfailures/")))
                HandleRRFlush(httpContextWrapper);

            else if (IsInRRContentDirectory(httpContextWrapper)
                && url.ToLowerInvariant().Contains("/dashboard/"))
                WriteDashboard(httpContextWrapper);
        }

        private void WriteDashboard(HttpContextBase httpContextWrapper)
        {
            var dashboardHtml = Resources.ResourceStrings.Dashboard;
            var config = RRContainer.Current.GetInstance<IRRConfiguration>();
            var ipfilter = RRContainer.Current.GetInstance<IIpFilter>();
            var user = httpContextWrapper.User == null ? string.Empty : httpContextWrapper.User.Identity.Name;
            if ((config.AuthorizedUserList.AllowsAnonymous() || config.AuthorizedUserList.Contains(user)) &&
                ipfilter.IsAuthorizedIpAddress(httpContextWrapper))
            {
                var transformedDashboard = TransformDashboard(dashboardHtml);
                httpContextWrapper.Response.Write(transformedDashboard);
            }
            else
                httpContextWrapper.Response.StatusCode = 401;
            if (httpContextWrapper.ApplicationInstance != null)
                httpContextWrapper.ApplicationInstance.CompleteRequest();
        }

        private static string TransformDashboard(string dashboard)
        {
            var queue = RRContainer.Current.GetInstance<IReducingQueue>();
            var repo = RRContainer.Current.GetInstance<IReductionRepository>(); 
            var uriBuilder = RRContainer.Current.GetInstance<IUriBuilder>();
            var config = RRContainer.Current.GetInstance<IRRConfiguration>();
            var transformed = dashboard.Replace("<%server%>", Environment.MachineName);
            transformed = transformed.Replace("<%app%>", AppDomain.CurrentDomain.BaseDirectory);
            transformed = transformed.Replace("<%version%>", Assembly.GetExecutingAssembly().GetName().Version.ToString());

            transformed = transformed.Replace("<%processedItem%>",
                                              queue.ItemBeingProcessed == null
                                                  ? "Shhhh. I'm Sleeping"
                                                  : ListUrls(queue.ItemBeingProcessed.Urls, "::"));
            var configProps = config.GetType().GetProperties();
            var configList = new StringBuilder();
            foreach (var item in configProps)
            {
                configList.AppendFormat("<tr><td{0}>{1}</td>",
                    item.Name == "JavaScriptUrlsToIgnore" ? " style='vertical-align: top;'" : string.Empty,
                    item.Name);
                var array = item.GetValue(config, null) as IEnumerable;
                if (array == null || item.PropertyType.IsAssignableFrom(typeof(string)))
                {
                    string value = Convert.ToString(item.GetValue(config, null));
                    string result;
                    switch (item.Name)
                    {
                        case "JavaScriptUrlsToIgnore":
                            result = ListUrls(value, ",");
                            break;
                        case "ConnectionStringName":
                            result = Regex.Replace(value, @";(PWD|PASSWORD)=\S+(;|$)", ";PASSWORD HIDDEN;", RegexOptions.Singleline | RegexOptions.IgnoreCase);
                            break;
                        default:
                            result = value;
                            break;
                    }
                    configList.AppendFormat("<td>{0}</td>", result);
                }
                else
                {
                    configList.Append("<td>");
                    foreach (var mem in array)
                    {
                        configList.Append(mem.ToString());
                        configList.Append("<br/>");
                    }
                    configList.Append("</td>");
                }
                configList.Append("</tr>");
            }
            transformed = transformed.Replace("<%configs%>", configList.ToString());
            var queueArray = queue.ToArray();
            var queueList = new StringBuilder();
            foreach (var item in queueArray)
            {
                if (queueList.Length > 0)
                {
                    queueList.AppendLine("<hr/>");
                }

                queueList.AppendLine("<ul>");

                var urlArray = item.Urls.Split(new[] {"::"}, StringSplitOptions.RemoveEmptyEntries);
                foreach (var url in urlArray)
                {
                    queueList.AppendFormat("<li>{0}</li>{1}", url, Environment.NewLine);
                }
                
                queueList.AppendFormat("</ul>");
            }
            transformed = transformed.Replace("<%queue%>", queueList.ToString());
            var repoArray = repo.ToArray();
            var repoList = new StringBuilder();
            foreach (var item in repoArray)
            {
                if (repoList.Length <= 0)
                {
                    repoList.AppendLine("<ul>");
                }
                repoList.AppendFormat("<li>{0} || <a href='{1}/flush/RRflush.aspx'>Flush</a></li>{2}",
                                      item, uriBuilder.ParseKey(item).RemoveDashes(), Environment.NewLine);
            }
            if (repoList.Length > 0)
            {
                repoList.AppendLine("</ul>");
            }
            transformed = transformed.Replace("<%repo%>", repoList.ToString());
            var failureList = new StringBuilder();
            foreach (var item in queue.Failures)
            {
                if (failureList.Length > 0)
                {
                    failureList.Append("<hr/>");
                }

                failureList.AppendFormat("Key: {0}<br/>", item.Key);
                failureList.AppendFormat("&nbsp;&nbsp;&nbsp;&nbsp;First errored on: {0} {1}<br/>",
                    item.Value.CreatedOn.ToShortDateString(), item.Value.CreatedOn.ToLongTimeString());
                if (item.Value.Count > 1)
                {
                    failureList.AppendFormat("&nbsp;&nbsp;&nbsp;&nbsp;Last errored on: {0} {1}<br/>",
                        item.Value.UpdatedOn.ToShortDateString(), item.Value.UpdatedOn.ToLongTimeString());
                }
                failureList.AppendFormat("&nbsp;&nbsp;&nbsp;&nbsp;Number: {0}<br/>", item.Value.Count);
                var exception = item.Value.Exception;
                int iterator = 0;
                while (exception != null)
                {
                    failureList.AppendFormat("&nbsp;&nbsp;&nbsp;&nbsp;Exception #{0}: {1}<br/>",
                        ++iterator, exception.Message);
                    if (!string.IsNullOrEmpty(exception.StackTrace))
                    {
                        failureList.AppendFormat("&nbsp;&nbsp;&nbsp;&nbsp;Stack Trace#{0}: <pre>{1}</pre><br/>",
                                                 iterator, exception.StackTrace);
                    }
                    exception = exception.InnerException;
                }
            }

            transformed = transformed.Replace("<%failures%>", failureList.ToString());
            return transformed;
        }

        public void HandleRRFlush(HttpContextBase httpContextWrapper)
        {
            var url = EnsurePath(httpContextWrapper.Request.RawUrl);
            if (!IsInRRContentDirectory(httpContextWrapper) 
                || (!url.ToLowerInvariant().Contains("/flush/")
                && !url.ToLowerInvariant().Contains("/flushfailures/"))) return;

            var config = RRContainer.Current.GetInstance<IRRConfiguration>();
            var hostingEnvironment = RRContainer.Current.GetInstance<IHostingEnvironmentWrapper>();
            var ipfilter = RRContainer.Current.GetInstance<IIpFilter>();
            if (string.IsNullOrEmpty(config.SpritePhysicalPath))
                config.SpritePhysicalPath = hostingEnvironment.MapPath(config.SpriteVirtualPath);
            var user = httpContextWrapper.User == null ? string.Empty : httpContextWrapper.User.Identity.Name;
            if ((config.AuthorizedUserList.AllowsAnonymous() || config.AuthorizedUserList.Contains(user)) &&
                ipfilter.IsAuthorizedIpAddress(httpContextWrapper))
            {
                if(url.ToLowerInvariant().Contains("/flushfailures/"))
                {
                    var queue = RRContainer.Current.GetInstance<IReducingQueue>();
                    queue.ClearFailures();
                    RRTracer.Trace("Failures Cleared");
                }
                else
                {
                    var uriBuilder = RRContainer.Current.GetInstance<IUriBuilder>();
                    var key = uriBuilder.ParseKey(url.ToLower().Replace("/flush/rrflush.aspx/", "-flush"));
                    if (key == Guid.Empty)
                        key = uriBuilder.ParseKey(url.ToLower().Replace("/flush/", "-flush"));
                    var store = RRContainer.Current.GetInstance<IStore>();
                    store.Flush(key);
                    RRTracer.Trace("{0} Flushed {1}", user, key);
                }
                if(HttpContext.Current != null)
                    httpContextWrapper.Response.Redirect(string.Format("{0}/dashboard", config.SpriteVirtualPath));
            }
            else
                httpContextWrapper.Response.StatusCode = 401;
        }

        public void HandleRRContent(HttpContextBase httpContextWrapper)
        {
            if (Registry.HandlerMaps.Count > 0)
            {
                foreach (var handler in Registry.HandlerMaps.Select(map => map(httpContextWrapper.Request.Url)).Where(handler => handler != null))
                {
                    if (HttpContext.Current != null)
                        HttpContext.Current.RemapHandler(handler); //can't use RemapHandler on HttpContextBase due to .net3.5 compat
                    else //unit testing
                        httpContextWrapper.Items["remapped handler"] = handler;
                    return;
                }
            }

            var url = httpContextWrapper.Request.RawUrl;
            var index = url.IndexOf('?');
            if (index > 0)
                url = url.Substring(0, index);

                                                                                                                                                                                                                                                                                                                                            var actionUrl = EnsurePath(url);
            if (!IsInRRContentDirectory(httpContextWrapper)
                || actionUrl.ToLowerInvariant().Contains("/flush/")
                || actionUrl.ToLowerInvariant().Contains("/flushfailures/")
                || actionUrl.ToLowerInvariant().Contains("/dashboard/"))
            {
                return;
            }

            var config = RRContainer.Current.GetInstance<IRRConfiguration>();
            var hostingEnvironment = RRContainer.Current.GetInstance<IHostingEnvironmentWrapper>();
            if (string.IsNullOrEmpty(config.SpritePhysicalPath))
                config.SpritePhysicalPath = hostingEnvironment.MapPath(config.SpriteVirtualPath);

            RRTracer.Trace("Beginning to serve {0}", url);
            var sig = RRContainer.Current.GetInstance<IUriBuilder>().ParseSignature(url);
            if (sig == Guid.Empty.RemoveDashes())
                return;
            var etag = httpContextWrapper.Request.Headers["If-None-Match"];
            etag = etag == null ? string.Empty : etag.Replace("\"", "");
            var store = RRContainer.Current.GetInstance<IStore>();
            if (sig == etag || store.SendContent(url, httpContextWrapper.Response))
            {
                httpContextWrapper.Response.Cache.SetETag(string.Format(@"""{0}""", sig));
                httpContextWrapper.Response.Cache.SetCacheability(HttpCacheability.Public);
                httpContextWrapper.Response.Expires = 60 * 24 * 360; //LITTLE under A YEAR
                if (sig == etag)
                    httpContextWrapper.Response.StatusCode = 304;
                else if (url.EndsWith(".css", StringComparison.OrdinalIgnoreCase))
                    httpContextWrapper.Response.ContentType = "text/css";
                else if (url.EndsWith(".js", StringComparison.OrdinalIgnoreCase))
                    httpContextWrapper.Response.ContentType = "application/x-javascript";
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
            var rrPath = EnsurePath(config.SpriteVirtualPath);
            var url = httpContextWrapper.Request.RawUrl;
            if(rrPath.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                url = httpContextWrapper.Request.Url.AbsoluteUri;
            return url.StartsWith(rrPath, StringComparison.OrdinalIgnoreCase);
        }

        public void InstallFilter(HttpContextBase context)
        {
            RRTracer.Trace("Entering Module");
            var request = context.Request;
            if (context.Response.ContentType != "text/html" || 
                request.RawUrl == "/favicon.ico" || 
                IsInRRContentDirectory(context))
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

        private static string ListUrls(string urls, string separator)
        {
            if (string.IsNullOrEmpty(urls))
            {
                return "-No urls to process-";
            }

            // Split the urls into unique items.
            var urlArray = urls.Split(new[] { separator }, StringSplitOptions.RemoveEmptyEntries);

            var processedItemsHtml = new StringBuilder();
            foreach (var url in urlArray)
            {
                if (processedItemsHtml.Length <= 0)
                    processedItemsHtml.AppendLine("<ul>");
                processedItemsHtml.AppendFormat("<li>{0}</li>{1}", url, Environment.NewLine);
            }

            if (processedItemsHtml.Length > 0)
                processedItemsHtml.AppendLine("</ul>");

            return processedItemsHtml.ToString();
        }
    }
}
