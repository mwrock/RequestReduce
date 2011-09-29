using System;
using System.Collections.Generic;
using System.Text;
using RequestReduce.Store;
using RequestReduce.Utilities;
using RequestReduce.Module;
using RequestReduce.ResourceTypes;

namespace RequestReduce.Reducer
{
    public class JavaScriptReducer : HeadResourceReducerBase<JavaScriptResource>
    {
        public JavaScriptReducer(IWebClientWrapper webClientWrapper, IStore store, IMinifier minifier, IUriBuilder uriBuilder) : base(webClientWrapper, store, minifier, uriBuilder)
        {
        }

        protected override string ProcessResource(Guid key, IEnumerable<string> urls)
        {
            var mergedJSBuilder = new StringBuilder();
            foreach (var url in urls)
                mergedJSBuilder.AppendLine(ProcessJavaScript(url));
            return mergedJSBuilder.ToString();
        }

        protected virtual string ProcessJavaScript(string url)
        {
            var jsContent = webClientWrapper.DownloadString<JavaScriptResource>(url);
            return jsContent;
        }
    }
}
