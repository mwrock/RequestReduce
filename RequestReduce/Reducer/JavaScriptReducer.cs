using System;
using System.Collections.Generic;
using System.Text;
using RequestReduce.Store;
using RequestReduce.Utilities;
using RequestReduce.Module;

namespace RequestReduce.Reducer
{
    public class JavaScriptReducer : IReducer
    {
        private readonly IWebClientWrapper webClientWrapper;
        private readonly IStore store;
        private IMinifier minifier;
        private readonly IUriBuilder uriBuilder;

        public ResourceType SupportedResourceType { get { return ResourceType.JavaScript; } }

        public JavaScriptReducer(IWebClientWrapper webClientWrapper, IStore store, IMinifier minifier, IUriBuilder uriBuilder)
        {
            this.webClientWrapper = webClientWrapper;
            this.uriBuilder = uriBuilder;
            this.minifier = minifier;
            this.store = store;
        }

        public virtual string Process(string urls)
        {
            var guid = Hasher.Hash(urls);
            return Process(guid, urls);
        }

        public virtual string Process(Guid key, string urls)
        {
            RRTracer.Trace("beginning reducing process for {0}", urls);
            var urlList = SplitUrls(urls);
            var mergedJSBuilder = new StringBuilder();
            foreach (var url in urlList)
                mergedJSBuilder.AppendLine(ProcessJavaScript(url));
            var mergedJS = mergedJSBuilder.ToString();
            var bytes = Encoding.UTF8.GetBytes(minifier.MinifyJavaScript(mergedJS));
            var virtualfileName = uriBuilder.BuildJavaScriptUrl(key, bytes);
            store.Save(bytes, virtualfileName, urls);
            RRTracer.Trace("finishing reducing process for {0}", urls);
            return virtualfileName;
        }

        protected virtual string ProcessJavaScript(string url)
        {
            var jsContent = webClientWrapper.DownloadJavaScriptString(url);
            return jsContent;
        }

        protected static IEnumerable<string> SplitUrls(string urls)
        {
            return urls.Split(new string[] { "::" }, StringSplitOptions.RemoveEmptyEntries);
        }

        public void Dispose()
        {
            store.Dispose();
        }
    }
}
