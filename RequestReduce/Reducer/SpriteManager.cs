using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RequestReduce.Configuration;
using RequestReduce.Store;
using RequestReduce.Utilities;
using RequestReduce.Module;

namespace RequestReduce.Reducer
{
    public class SpriteManager : ISpriteManager
    {
        protected ISpriteContainer SpriteContainer = null;
        private IWebClientWrapper webClientWrapper = null;
        private IRRConfiguration config = null;
        private readonly IUriBuilder uriBuilder;
        private readonly IStore store;
        private readonly IPngOptimizer pngOptimizer;
        protected IDictionary<ImageMetadata, SpritedImage> spriteList = new Dictionary<ImageMetadata, SpritedImage>();

        public SpriteManager(IWebClientWrapper webClientWrapper, IRRConfiguration config, IUriBuilder uriBuilder, IStore store, IPngOptimizer pngOptimizer)
        {
            this.webClientWrapper = webClientWrapper;
            this.uriBuilder = uriBuilder;
            this.store = store;
            this.pngOptimizer = pngOptimizer;
            this.config = config;
            SpriteContainer = new SpriteContainer(webClientWrapper, config);
        }

        public virtual void Add(BackgroundImageClass image)
        {
            var imageKey = new ImageMetadata(image);
            if (spriteList.ContainsKey(imageKey))
                return;
            var spritedImage = SpriteContainer.AddImage(image);
            spriteList.Add(imageKey, spritedImage);
            if (SpriteContainer.Size >= config.SpriteSizeLimit || (SpriteContainer.Colors >= config.SpriteColorLimit && !config.ImageQuantizationDisabled && !config.ImageOptimizationDisabled))
                Flush();
        }

        public virtual void Flush()
        {
            if(SpriteContainer.Size > 0)
            {
                using (var spriteWriter = new SpriteWriter(SpriteContainer.Width, SpriteContainer.Height))
                {
                    var offset = 0;
                    foreach (var image in SpriteContainer)
                    {
                        spriteWriter.WriteImage(image.Image);
                        image.Position = offset;
                        offset += image.Image.Width + 1;
                    }
                    var bytes = spriteWriter.GetBytes("image/png");
                    byte[] optBytes = null;
                    try
                    {
                        optBytes = (config.ImageOptimizationDisabled || !config.IsFullTrust) ? bytes : pngOptimizer.OptimizePng(bytes, config.ImageOptimizationCompressionLevel, config.ImageQuantizationDisabled);
                    }
                    catch (OptimizationException optEx)
                    {
                        optBytes = bytes;
                        RRTracer.Trace(string.Format("Errors optimizing {0}. Received Error: {1}", SpritedCssKey, optEx.Message));
                        if (RequestReduceModule.CaptureErrorAction != null)
                            RequestReduceModule.CaptureErrorAction(optEx);
                    }
                    var url = GetSpriteUrl(optBytes);
                    store.Save(optBytes, url, null);
                    foreach (var image in SpriteContainer)
                        image.Url = url;
                }
            }
            SpriteContainer.Dispose();
            SpriteContainer = new SpriteContainer(webClientWrapper, config);
            return;
        }

        public Guid SpritedCssKey { get; set; }

        private string GetSpriteUrl(byte[] bytes)
        {
            if (SpritedCssKey == Guid.Empty)
                throw new InvalidOperationException("The SpritedCssKey must be set before using the SprieManager.");
            return uriBuilder.BuildSpriteUrl(SpritedCssKey, bytes);
        }

        public IEnumerator<SpritedImage> GetEnumerator()
        {
            return spriteList.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        protected struct ImageMetadata
        {
            public ImageMetadata(BackgroundImageClass image) : this()
            {
                Url = image.ImageUrl;
                Width = image.Width ?? 0;
                Height = image.Height ?? 0;
                XOffset = image.XOffset.Offset;
                YOffset = image.YOffset.Offset;
            }

            public int Width { get; set; }
            public int Height { get; set; }
            public int XOffset { get; set; }
            public int YOffset { get; set; }
            public string Url { get; set; }
        }
    }
}