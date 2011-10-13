using System;
using System.Collections.Generic;
using System.Text;
using RequestReduce.Store;
using RequestReduce.Utilities;
using RequestReduce.Module;
using RequestReduce.ResourceTypes;
using System.IO;
using RequestReduce.Configuration;

namespace RequestReduce.Reducer
{
    public class JavaScriptReducer : HeadResourceReducerBase<JavaScriptResource>
    {
        private readonly IRRConfiguration config;

        public JavaScriptReducer(IWebClientWrapper webClientWrapper, IStore store, IMinifier minifier, IUriBuilder uriBuilder, IRRConfiguration config) : base(webClientWrapper, store, minifier, uriBuilder)
        {
            this.config = config;
        }

        protected override string ProcessResource(Guid key, IEnumerable<string> urls)
        {
            var mergedJSBuilder = new StringBuilder();
            foreach (var url in urls)
            {
                mergedJSBuilder.Append(ProcessJavaScript(url));
                if (mergedJSBuilder.Length > 0 && (mergedJSBuilder[mergedJSBuilder.Length - 1] == ')' || mergedJSBuilder[mergedJSBuilder.Length - 1] == '}'))
                    mergedJSBuilder.Append(";");
                mergedJSBuilder.AppendLine();
            }
            return mergedJSBuilder.ToString();
        }

        protected virtual string ProcessJavaScript(string url)
        {
            string jsContent = string.Empty;
            using (var response = webClientWrapper.Download<JavaScriptResource>(url))
            {
                if (response == null)
                    return null;
                var expires = response.Headers["Expires"];
                try
                {
                    if (!string.IsNullOrEmpty(expires) && DateTime.Parse(expires) < DateTime.Now.AddDays(6))
                        AddUrlToIgnores(url);
                }
                catch (FormatException) { };

                var cacheControl = response.Headers["Cache-Control"];
                if(!string.IsNullOrEmpty(cacheControl)) 
                {
                    var cacheControlVals = cacheControl.ToLower().Split(new char[] {','}, StringSplitOptions.RemoveEmptyEntries);
                    foreach(var val in cacheControlVals)
                    {
                        try
                        {
                            if((val.Contains("no-") || (val.Contains("max-age") && Int32.Parse(val.Remove(0, 8)) < (60*60*24*7))))
                                AddUrlToIgnores(url);
                        }
                        catch(FormatException){}
                    }
                }
                using (var responseStream = response.GetResponseStream())
                {
                    if (responseStream != null)
                    {
                        using (var streameader = new StreamReader(responseStream, Encoding.UTF8))
                        {
                            jsContent = streameader.ReadToEnd();
                        }
                    }
                }
            }
            return jsContent;
        }

        private void AddUrlToIgnores(string url)
        {
            var uriBuilder = new System.UriBuilder(url);
            var urlToIgnore = url.Replace(uriBuilder.Query.Length == 0 ? "?" : uriBuilder.Query, "").Replace(uriBuilder.Scheme + "://", "");
            if (!config.JavaScriptUrlsToIgnore.ToLower().Contains(urlToIgnore.ToLower()))
                config.JavaScriptUrlsToIgnore += "," + urlToIgnore;
        }
    }
}
