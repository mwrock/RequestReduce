﻿using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using RequestReduce.Api;
using RequestReduce.Configuration;
using RequestReduce.IOC;
using RequestReduce.Properties;
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
            context.PostAuthenticateRequest += (sender, e) => HandleAuthenticatedActions(new HttpContextWrapper(((HttpApplication)sender).Context));
        }

        private void HandleAuthenticatedActions(HttpContextWrapper httpContextWrapper)
        {
            var url = EnsurePath(httpContextWrapper.Request.RawUrl);
            if (IsInRRContentDirectory(httpContextWrapper) && (
                url.EndsWith("/flush/", StringComparison.OrdinalIgnoreCase)
                || url.EndsWith("/flushfailures/", StringComparison.OrdinalIgnoreCase)))
                HandleRRFlush(httpContextWrapper);

            else if (IsInRRContentDirectory(httpContextWrapper)
                && url.EndsWith("/dashboard/", StringComparison.OrdinalIgnoreCase))
                WriteDashboard(httpContextWrapper);
        }

        private void WriteDashboard(HttpContextBase httpContextWrapper)
        {
            var dashboardHtml = Resources.Dashboard;
            var config = RRContainer.Current.GetInstance<IRRConfiguration>();
            var user = httpContextWrapper.User.Identity.Name;
            if (config.AuthorizedUserList.AllowsAnonymous() || config.AuthorizedUserList.Contains(user))
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
            transformed = transformed.Replace("<%processedItem%>", queue.ItemBeingProcessed == null ? "Shhhh. I'm Sleeping" : queue.ItemBeingProcessed.Urls);
            var configProps = config.GetType().GetProperties();
            var configList = new StringBuilder();
            foreach (var item in configProps)
            {
                configList.Append("<tr><td>");
                configList.Append(item.Name);
                configList.Append("</td><td>");
                var array = item.GetValue(config, null) as IEnumerable;
                if(array == null || item.PropertyType.IsAssignableFrom(typeof(string)))
                    configList.Append(item.GetValue(config, null));
                else
                {
                    foreach (var mem in array)
                    {
                        configList.Append(mem.ToString());
                        configList.Append("<br/>");
                    }
                }
                configList.Append("</td></tr>");
            }
            transformed = transformed.Replace("<%configs%>", configList.ToString());
            var queueArray = queue.ToArray();
            var queueList = new StringBuilder();
            foreach (var item in queueArray)
            {
                queueList.Append(item.Urls);
                queueList.Append("<br/>");
            }
            transformed = transformed.Replace("<%queue%>", queueList.ToString());
            var repoArray = repo.ToArray();
            var repoList = new StringBuilder();
            foreach (var item in repoArray)
            {
                repoList.Append(item);
                repoList.Append(string.Format(" <a href='{0}/flush'>Flush</a>", uriBuilder.ParseKey(item).RemoveDashes()));
                repoList.Append("<br/>");
            }
            transformed = transformed.Replace("<%repo%>", repoList.ToString());
            var failures = queue.Failures;
            var failureList = new StringBuilder();
            foreach (var item in failures)
            {
                failureList.Append("key: ");
                failureList.Append(item.Key);
                failureList.Append(" Number: ");
                failureList.Append(item.Value);
                failureList.Append("<br/>");
            }
            transformed = transformed.Replace("<%failures%>", failureList.ToString());
            return transformed;
        }

        public void HandleRRFlush(HttpContextBase httpContextWrapper)
        {
            var url = EnsurePath(httpContextWrapper.Request.RawUrl);
            if (!IsInRRContentDirectory(httpContextWrapper) 
                || (!url.EndsWith("/flush/", StringComparison.OrdinalIgnoreCase)
                && !url.EndsWith("/flushfailures/", StringComparison.OrdinalIgnoreCase))) return;

            var config = RRContainer.Current.GetInstance<IRRConfiguration>();
            if (string.IsNullOrEmpty(config.SpritePhysicalPath))
                config.SpritePhysicalPath = httpContextWrapper.Server.MapPath(config.SpriteVirtualPath);
            var user = httpContextWrapper.User.Identity.Name;
            if (config.AuthorizedUserList.AllowsAnonymous() || config.AuthorizedUserList.Contains(user))
            {
                if(url.EndsWith("/flushfailures/", StringComparison.OrdinalIgnoreCase))
                {
                    var queue = RRContainer.Current.GetInstance<IReducingQueue>();
                    queue.ClearFailures();
                    RRTracer.Trace("Failures Cleared");
                }
                else
                {
                    var store = RRContainer.Current.GetInstance<IStore>();
                    var uriBuilder = RRContainer.Current.GetInstance<IUriBuilder>();
                    var key = uriBuilder.ParseKey(url.ToLower().Replace("/flush/", "-flush"));
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
            var actionUrl = EnsurePath(url);
            if (!IsInRRContentDirectory(httpContextWrapper)
                || actionUrl.EndsWith("/flush/", StringComparison.OrdinalIgnoreCase)
                || actionUrl.EndsWith("/flushfailures/", StringComparison.OrdinalIgnoreCase)
                || actionUrl.EndsWith("/dashboard/", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var config = RRContainer.Current.GetInstance<IRRConfiguration>();
            if (string.IsNullOrEmpty(config.SpritePhysicalPath))
                config.SpritePhysicalPath = httpContextWrapper.Server.MapPath(config.SpriteVirtualPath);

            RRTracer.Trace("Beginning to serve {0}", url);
            var store = RRContainer.Current.GetInstance<IStore>();
            var sig = RRContainer.Current.GetInstance<IUriBuilder>().ParseSignature(url);
            if (sig == Guid.Empty.RemoveDashes())
                return;
            var etag = httpContextWrapper.Request.Headers["If-None-Match"];
            etag = etag == null ? string.Empty : etag.Replace("\"", "");
            if (sig == etag || store.SendContent(url, httpContextWrapper.Response))
            {
                httpContextWrapper.Response.Cache.SetETag(string.Format(@"""{0}""", sig));
                httpContextWrapper.Response.Cache.SetCacheability(HttpCacheability.Public);
                httpContextWrapper.Response.Expires = 60*24*360; //LITTLE under A YEAR
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
            var config = RRContainer.Current.GetInstance<IRRConfiguration>();
            if (context.Items.Contains(CONTEXT_KEY) || 
                context.Response.ContentType != "text/html" || 
                (request.QueryString["RRFilter"] != null && request.QueryString["RRFilter"].Equals("disabled", StringComparison.OrdinalIgnoreCase)) || 
                (config.CssProcessingDisabled && config.JavaScriptProcessingDisabled) ||
                request.RawUrl == "/favicon.ico" || 
                RRContainer.Current.GetAllInstances<IFilter>().Where(x => x is PageFilter).FirstOrDefault(y => y.IgnoreTarget(new PageFilterContext(context.Request))) != null ||
                IsInRRContentDirectory(context))
                return;

            if(string.IsNullOrEmpty(config.SpritePhysicalPath))
                config.SpritePhysicalPath = context.Server.MapPath(config.SpriteVirtualPath);

            var oldFilter = context.Response.Filter;
            context.Response.Filter = RRContainer.Current.GetInstance<AbstractFilter>();
            context.Items.Add(CONTEXT_KEY, new object());
            RRTracer.Trace("Attaching Filter to {0}", request.RawUrl);
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
