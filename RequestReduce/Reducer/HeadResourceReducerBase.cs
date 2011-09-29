using System;
using System.Collections.Generic;
using System.Text;
using RequestReduce.Store;
using RequestReduce.Utilities;
using RequestReduce.Module;
using RequestReduce.ResourceTypes;

namespace RequestReduce.Reducer
{
    public abstract class HeadResourceReducerBase<T> : IReducer where T : IResourceType, new()
    {
        protected readonly IWebClientWrapper webClientWrapper;
        private readonly IStore store;
        private IMinifier minifier;
        private readonly IUriBuilder uriBuilder;

        public Type SupportedResourceType { get { return typeof(T); } }

        public HeadResourceReducerBase(IWebClientWrapper webClientWrapper, IStore store, IMinifier minifier, IUriBuilder uriBuilder)
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
            var processedResource = ProcessResource(key, urlList);
            var bytes = Encoding.UTF8.GetBytes(minifier.Minify<T>(processedResource));
            var virtualfileName = uriBuilder.BuildResourceUrl<T>(key, bytes);
            store.Save(bytes, virtualfileName, urls);
            RRTracer.Trace("finishing reducing process for {0}", urls);
            return virtualfileName;
        }

        protected abstract string ProcessResource(Guid key, IEnumerable<string> urls);

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
