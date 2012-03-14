using System;
using System.Collections.Generic;
using System.Text;
using RequestReduce.Store;
using RequestReduce.Utilities;
using RequestReduce.ResourceTypes;
using System.IO;
using RequestReduce.Configuration;
using UriBuilder = System.UriBuilder;

namespace RequestReduce.Reducer
{
    public class JavaScriptReducer : HeadResourceReducerBase<JavaScriptResource>
    {
        private readonly IRRConfiguration config;
        private readonly IResponseDecoder responseDecoder;

        public JavaScriptReducer(IWebClientWrapper webClientWrapper, IStore store, IMinifier minifier, IUriBuilder uriBuilder, IRRConfiguration config, IResponseDecoder responseDecoder) : base(webClientWrapper, store, minifier, uriBuilder)
        {
            this.config = config;
            this.responseDecoder = responseDecoder;
        }

        protected override string ProcessResource(Guid key, IEnumerable<string> urls, string host)
        {
            RRTracer.Trace("beginning to merge js");
            try
            {
                var mergedJSBuilder = new StringBuilder();
                foreach (var url in urls)
                {
                    mergedJSBuilder.Append(ProcessJavaScript(url).Trim());
                    if (mergedJSBuilder.Length > 0 && (mergedJSBuilder[mergedJSBuilder.Length - 1] != ';'))
                    {
                        mergedJSBuilder.Append(";");
                        RRTracer.Trace("Appending missing semicolon to {0}", url);
                    }
                    mergedJSBuilder.AppendLine();
                }
                RRTracer.Trace("finished merging js with a length of {0}", mergedJSBuilder.Length);
                return mergedJSBuilder.ToString();
            }
            catch(NearFutureJavascriptException)
            {
                RRTracer.Trace("catching near future exception");
                return null;
            }
        }

        protected virtual string ProcessJavaScript(string url)
        {
            RRTracer.Trace("Beginning Processing {0}", url);
            string jsContent = string.Empty;
            url = url.Replace("&amp;", "&");
            RRTracer.Trace("Beginning to Download {0}", url);
            using (var response = WebClientWrapper.Download<JavaScriptResource>(url))
            {
                if (response == null)
                {
                    RRTracer.Trace("Response is null for {0}", url);
                    return null;
                }
                var expires = response.Headers["Expires"];
                try
                {
                    if (!string.IsNullOrEmpty(expires) && DateTime.Parse(expires) < DateTime.Now.AddDays(6))
                    {
                        RRTracer.Trace("{0} expires in less than a week", url);
                        AddUrlToIgnores(url);
                    }
                }
                catch (FormatException) { RRTracer.Trace("Format exception thrown parsing expires of {0}", url); }

                var cacheControl = response.Headers["Cache-Control"];
                if(!string.IsNullOrEmpty(cacheControl)) 
                {
                    var cacheControlVals = cacheControl.ToLower().Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);
                    foreach(var val in cacheControlVals)
                    {
                        try
                        {
                            if((val.Contains("no-") || (val.Contains("max-age") && Int32.Parse(val.Trim().Remove(0, 8)) < (60*60*24*7))))
                            {
                                RRTracer.Trace("{0} max-age in less than a week", url);
                                AddUrlToIgnores(url);
                            }
                        }
                        catch (FormatException) { RRTracer.Trace("Format exception thrown parsing max-age of {0}", url); }
                    }
                }
                using (var responseStream = response.GetResponseStream())
                {
                    if (responseStream != null)
                    {
                        using (var streamDecoder = responseDecoder.GetDecodableStream(responseStream, response.Headers["Content-Encoding"]))
                        {
                            using (var streameader = new StreamReader(streamDecoder, Encoding.UTF8))
                            {
                                jsContent = streameader.ReadToEnd();
                                RRTracer.Trace("content of {0} read and has {1} bytes and a length of {2}", url, response.ContentLength, jsContent.Length);
                            }
                        }
                    }
                }
            }
            return jsContent;
        }

        private void AddUrlToIgnores(string url)
        {
            var uriBuilder = new UriBuilder(url);
            var urlToIgnore = url.Replace(uriBuilder.Query.Length == 0 ? "?" : uriBuilder.Query, "").Replace(uriBuilder.Scheme + "://", "");
            if (!config.JavaScriptUrlsToIgnore.ToLower().Contains(urlToIgnore.ToLower()))
                config.JavaScriptUrlsToIgnore += "," + urlToIgnore;
            throw new NearFutureJavascriptException();
        }

        private class NearFutureJavascriptException : Exception
        {
        }
    }
}
