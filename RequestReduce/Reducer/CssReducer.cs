using System;
using System.Collections.Generic;
using System.Text;
using RequestReduce.Store;
using RequestReduce.Utilities;
using RequestReduce.Module;
using RequestReduce.ResourceTypes;

namespace RequestReduce.Reducer
{
    public class CssReducer : HeadResourceReducerBase<CssResource>
    {
        private ISpriteManager spriteManager;
        private ICssImageTransformer cssImageTransformer;

        public CssReducer(IWebClientWrapper webClientWrapper, IStore store, IMinifier minifier, ISpriteManager spriteManager, ICssImageTransformer cssImageTransformer, IUriBuilder uriBuilder) : base(webClientWrapper, store, minifier, uriBuilder)
        {
            this.cssImageTransformer = cssImageTransformer;
            this.spriteManager = spriteManager;
        }

        protected override string ProcessResource(Guid key, IEnumerable<string> urls)
        {
            spriteManager.SpritedCssKey = key;
            var mergedCss = new StringBuilder();
            var imageUrls = new List<BackgroundImageClass>();
            foreach (var url in urls)
                mergedCss.Append(ProcessCss(url, imageUrls));
            foreach (var imageUrl in imageUrls)
                spriteManager.Add(imageUrl);
            spriteManager.Flush();
            return SpriteCss(mergedCss.ToString(), imageUrls);
        }

        protected virtual string ProcessCss(string url, List<BackgroundImageClass> imageUrls)
        {
            string cssContent = webClientWrapper.DownloadString<CssResource>(url);
            imageUrls.AddRange(cssImageTransformer.ExtractImageUrls(ref cssContent, url));
            return cssContent;
        }

        protected virtual string SpriteCss(string css, List<BackgroundImageClass> imageUrls)
        {
            foreach (var spritedImage in spriteManager)
                css = cssImageTransformer.InjectSprite(css, spritedImage);
            return css;
        }
    }
}
