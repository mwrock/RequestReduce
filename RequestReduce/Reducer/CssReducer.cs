using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using RequestReduce.Configuration;
using RequestReduce.Store;
using RequestReduce.Utilities;
using RequestReduce.ResourceTypes;

namespace RequestReduce.Reducer
{
    public class CssReducer : HeadResourceReducerBase<CssResource>
    {
        private readonly ISpriteManager spriteManager;
        private readonly ICssImageTransformer cssImageTransformer;
        private readonly IRRConfiguration configuration;
        private static readonly RegexCache Regex = new RegexCache();

        public CssReducer(IWebClientWrapper webClientWrapper, IStore store, IMinifier minifier, ISpriteManager spriteManager, ICssImageTransformer cssImageTransformer, IUriBuilder uriBuilder, IRRConfiguration configuration) : base(webClientWrapper, store, minifier, uriBuilder)
        {
            this.cssImageTransformer = cssImageTransformer;
            this.configuration = configuration;
            this.spriteManager = spriteManager;
        }

        protected override string ProcessResource(Guid key, IEnumerable<string> urls)
        {
            spriteManager.SpritedCssKey = key;
            var mergedCss = new StringBuilder();
            RRTracer.Trace("beginning to merge css");
            foreach (var url in urls)
                mergedCss.Append(ProcessCss(url));
            RRTracer.Trace("Finished merging css");
            spriteManager.Dispose();
            return SpriteCss(mergedCss.ToString());
        }

        protected virtual string ProcessCss(string url)
        {
            RRTracer.Trace("Beginning Processing {0}", url);
            var urlParts = url.Split(new[] {'|'}, 2);
            url = urlParts[0];
            RRTracer.Trace("Beginning to Download {0}", url);
            var cssContent = webClientWrapper.DownloadString<CssResource>(url);
            RRTracer.Trace("Finished Downloading {0}", url);
            RRTracer.Trace("Beginning to absolutize urls in {0}", url);
            cssContent = MakeRelativeUrlsAbsoluteAndRemoveComments(cssContent, url);
            RRTracer.Trace("finished absolutizing urls in {0}", url);
            RRTracer.Trace("Beginning to expand imports in {0}", url);
            cssContent = ExpandImports(cssContent, url);
            RRTracer.Trace("Finished expanding imports in {0}", url);
            if (!configuration.ImageSpritingDisabled)
            {
                RRTracer.Trace("Beginning to process sprites in {0}", url);
                ProcessSprites(cssContent);
                RRTracer.Trace("Finished processing sprites in {0}", url);
            }
            if (urlParts.Length > 1)
            {
                RRTracer.Trace("Beginning to wrap media in {0}", url);
                cssContent = WrapMedia(cssContent, urlParts[1]);
                RRTracer.Trace("finished wraping media in {0}", url);
            }
            return cssContent;
        }

        private string WrapMedia(string cssContent, string media)
        {
            return string.Format("@media {0} {{{1}}}", media, cssContent);
        }

        private string ExpandImports(string cssContent, string parentUrl)
        {
            var imports = Regex.CssImportPattern.Matches(cssContent);
            foreach (Match match in imports)
            {
                var url = match.Groups["url"].Value;
                var absoluteUrl = RelativeToAbsoluteUtility.ToAbsolute(parentUrl, url);
                var importContent = webClientWrapper.DownloadString<CssResource>(absoluteUrl);
                importContent = MakeRelativeUrlsAbsoluteAndRemoveComments(importContent, absoluteUrl);
                importContent = ExpandImports(importContent, absoluteUrl);
                var media = match.Groups["media"];
                if (media.Success)
                    importContent = WrapMedia(importContent, media.Value);
                cssContent = cssContent.Replace(match.ToString(), importContent);
            }
            return cssContent;
        }

        private void ProcessSprites(string cssContent)
        {
            var newImages = cssImageTransformer.ExtractImageUrls(cssContent);
            foreach (var imageUrl in newImages)
            {
                RRTracer.Trace("Adding {0}", imageUrl.ImageUrl);
                spriteManager.Add(imageUrl);
                RRTracer.Trace("Finished adding {0}", imageUrl.ImageUrl);
            }
        }

        protected virtual string SpriteCss(string css)
        {
            return spriteManager.Aggregate(css, (current, spritedImage) => cssImageTransformer.InjectSprite(current, spritedImage));
        }

        private string MakeRelativeUrlsAbsoluteAndRemoveComments(string originalCss, string parentCssUrl)
        {
            originalCss = Regex.CssCommentPattern.Replace(originalCss, string.Empty);
            var matches = Regex.ImageUrlPattern.Matches(originalCss);
            foreach (Match match in matches)
            {
                var url = match.Groups["url"].Value.Replace("'", "").Replace("\"", "").Trim();
                if (url.Length <= 0 || url.StartsWith("data:", StringComparison.OrdinalIgnoreCase)) continue;
                var newUrl = RelativeToAbsoluteUtility.ToAbsolute(parentCssUrl, url);
                originalCss = originalCss.Replace(match.Value, match.Value.Replace(url, newUrl));
            }
            return originalCss;
        }
    }
}
