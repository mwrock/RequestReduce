using System;
using System.Collections.Generic;
using System.Text;
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
            var mergedCss = new StringBuilder();
            var imageUrls = new List<BackgroundImageClass>();
            foreach (var url in urlList)
                mergedCss.Append(ProcessCss(url, imageUrls));
            foreach (var imageUrl in imageUrls)
                spriteManager.Add(imageUrl);
            spriteManager.Flush();
            var spritedCss = SpriteCss(mergedCss.ToString(), imageUrls);
            var bytes = Encoding.UTF8.GetBytes(minifier.Minify(spritedCss));
            var virtualfileName = uriBuilder.BuildCssUrl(key, bytes);
            store.Save(bytes, virtualfileName, urls);
            RRTracer.Trace("finishing reducing process for {0}", urls);
            return virtualfileName;
        }

        protected virtual string ProcessCss(string url, List<BackgroundImageClass> imageUrls)
        {
            var cssContent = webClientWrapper.DownloadString(url, true);
            imageUrls.AddRange(cssImageTransformer.ExtractImageUrls(ref cssContent, url));
            return cssContent;
        }

        protected virtual string SpriteCss(string css, List<BackgroundImageClass> imageUrls)
        {
            foreach (var imageUrl in imageUrls)
                css = cssImageTransformer.InjectSprite(css, imageUrl, spriteManager[imageUrl]);
            return css;
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
