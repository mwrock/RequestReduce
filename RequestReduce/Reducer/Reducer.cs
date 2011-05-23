using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using RequestReduce.Configuration;
using RequestReduce.Utilities;

namespace RequestReduce.Reducer
{
    public class Reducer : IReducer
    {
        private readonly IWebClientWrapper webClientWrapper;
        private IRRConfiguration config;
        private IFileWrapper fileWrapper;
        private IMinifier minifier;
        private ISpriteManager spriteManager;
        private ICssImageTransformer cssImageTransformer;

        public Reducer(IWebClientWrapper webClientWrapper, IRRConfiguration config, IFileWrapper fileWrapper, IMinifier minifier, ISpriteManager spriteManager, ICssImageTransformer cssImageTransformer)
        {
            this.webClientWrapper = webClientWrapper;
            this.cssImageTransformer = cssImageTransformer;
            this.spriteManager = spriteManager;
            this.minifier = minifier;
            this.fileWrapper = fileWrapper;
            this.config = config;
        }

        public virtual string Process(string urls)
        {
            var urlList = SplitUrls(urls);
            var guid = Guid.NewGuid();
            var virtualfileName = string.Format("{0}/{1}.css", config.SpriteVirtualPath, guid);
            var fileName = string.Format("{0}\\{1}.css", config.SpritePhysicalPath, guid);
            var mergedCss = new StringBuilder();
            foreach (var url in urlList)
                mergedCss.Append(ProcessCss(url));
            spriteManager.Flush();
            fileWrapper.Save(minifier.Minify(mergedCss.ToString()), fileName);
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
