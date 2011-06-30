using System;
using System.Collections.Generic;
using System.Text;
using RequestReduce.Configuration;
using RequestReduce.Store;
using RequestReduce.Utilities;

namespace RequestReduce.Reducer
{
    public class Reducer : IReducer
    {
        private readonly IWebClientWrapper webClientWrapper;
        private readonly IStore store;
        private IMinifier minifier;
        private ISpriteManager spriteManager;
        private ICssImageTransformer cssImageTransformer;
        private readonly IUriBuilder uriBuilder;

        public Reducer(IWebClientWrapper webClientWrapper, IStore store, IMinifier minifier, ISpriteManager spriteManager, ICssImageTransformer cssImageTransformer, IUriBuilder uriBuilder)
        {
            this.webClientWrapper = webClientWrapper;
            this.cssImageTransformer = cssImageTransformer;
            this.uriBuilder = uriBuilder;
            this.spriteManager = spriteManager;
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
            spriteManager.SpritedCssKey = key;
            var urlList = SplitUrls(urls);
            var virtualfileName = uriBuilder.BuildCssUrl(key);
            var mergedCss = new StringBuilder();
            foreach (var url in urlList)
                mergedCss.Append(ProcessCss(url));
            spriteManager.Flush();
            store.Save(Encoding.UTF8.GetBytes(minifier.Minify(mergedCss.ToString())), virtualfileName, urls);
            RRTracer.Trace("finishing reducing process for {0}", urls);
            return virtualfileName;
        }

        protected virtual string ProcessCss(string url)
        {
            var cssContent = webClientWrapper.DownloadString(url);
            var imageUrls = cssImageTransformer.ExtractImageUrls(ref cssContent, url);
            foreach (var imageUrl in imageUrls)
            {
                var sprite = spriteManager.Add(imageUrl);
                cssContent = cssImageTransformer.InjectSprite(cssContent, imageUrl, sprite);
            }
            return cssContent;
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
