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
        private IRRConfiguration config;
        private readonly IStore store;
        private IMinifier minifier;
        private ISpriteManager spriteManager;
        private ICssImageTransformer cssImageTransformer;

        public Reducer(IWebClientWrapper webClientWrapper, IRRConfiguration config, IStore store, IMinifier minifier, ISpriteManager spriteManager, ICssImageTransformer cssImageTransformer)
        {
            this.webClientWrapper = webClientWrapper;
            this.cssImageTransformer = cssImageTransformer;
            this.spriteManager = spriteManager;
            this.minifier = minifier;
            this.config = config;
            this.store = store;
        }

        public virtual string Process(string urls)
        {
            var guid = Hasher.Hash(urls);
            return Process(guid, urls);
        }

        public virtual string Process(Guid key, string urls)
        {
            spriteManager.SpritedCssKey = key;
            var urlList = SplitUrls(urls);
            var virtualfileName = string.Format("{0}/{1}/RequestReducedStyle.css", config.SpriteVirtualPath, key);
            var mergedCss = new StringBuilder();
            foreach (var url in urlList)
                mergedCss.Append(ProcessCss(url));
            spriteManager.Flush();
            store.Save(Encoding.UTF8.GetBytes(minifier.Minify(mergedCss.ToString())), virtualfileName);
            return virtualfileName;
        }

        protected virtual string ProcessCss(string url)
        {
            var cssContent = webClientWrapper.DownloadString(url);
            var imageUrls = cssImageTransformer.ExtractImageUrls(cssContent);
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

    }
}
