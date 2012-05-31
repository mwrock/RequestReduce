using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using RequestReduce.Configuration;
using RequestReduce.Utilities;
using RequestReduce.Module;

namespace RequestReduce.Handlers
{
    public class DashboardHandler : IHttpHandler
    {
        private readonly IRRConfiguration config;
        private readonly IIpFilter ipFilter;
        private readonly IReducingQueue queue;
        private readonly IReductionRepository repo;
        private readonly IUriBuilder uriBuilder;

        public DashboardHandler(IRRConfiguration config, IIpFilter ipFilter, IReducingQueue queue, IReductionRepository repo, IUriBuilder uriBuilder)
        {
            this.config = config;
            this.ipFilter = ipFilter;
            this.queue = queue;
            this.repo = repo;
            this.uriBuilder = uriBuilder;
        }

        public void ProcessRequest(HttpContext context)
        {
            ProcessRequest(new HttpContextWrapper(context));
        }

        public void ProcessRequest(HttpContextBase context)
        {
            var dashboardHtml = Resources.ResourceStrings.Dashboard;
            var user = context.User == null ? string.Empty : context.User.Identity.Name;
            if ((config.AuthorizedUserList.AllowsAnonymous() || config.AuthorizedUserList.Contains(user)) &&
                ipFilter.IsAuthorizedIpAddress(context))
            {
                var transformedDashboard = TransformDashboard(dashboardHtml);
                context.Response.Write(transformedDashboard);
            }
            else
                context.Response.StatusCode = 401;
            if (context.ApplicationInstance != null)
                context.ApplicationInstance.CompleteRequest();
        }

        private string TransformDashboard(string dashboard)
        {
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

                var urlArray = item.Urls.Split(new[] { "::" }, StringSplitOptions.RemoveEmptyEntries);
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

        public bool IsReusable
        {
            get { return true; }
        }
    }
}
