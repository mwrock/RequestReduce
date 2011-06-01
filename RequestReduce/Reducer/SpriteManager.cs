using System;
using System.Collections.Generic;
using System.Web;
using RequestReduce.Configuration;
using RequestReduce.Utilities;

namespace RequestReduce.Reducer
{
    public class SpriteManager : ISpriteManager
    {
        protected ISpriteContainer SpriteContainer = null;
        private IWebClientWrapper webClientWrapper = null;
        private IRRConfiguration config = null;
        private readonly ISpriteWriterFactory spriteWriterFactory;
        private readonly IUriBuilder uriBuilder;
        private IDictionary<ImageMetadata, Sprite> spriteList = new Dictionary<ImageMetadata, Sprite>();
        private int spriteIndex = 1;

        public SpriteManager(IWebClientWrapper webClientWrapper, IRRConfiguration config, ISpriteWriterFactory spriteWriterFactory, IUriBuilder uriBuilder)
        {
            this.webClientWrapper = webClientWrapper;
            this.spriteWriterFactory = spriteWriterFactory;
            this.uriBuilder = uriBuilder;
            this.config = config;
            SpriteContainer = new SpriteContainer(webClientWrapper);
        }

        public virtual Sprite this[BackgroundImageClass image]
        {
            get
            {
                var imageKey = new ImageMetadata(image);
                return spriteList.ContainsKey(imageKey) ? spriteList[imageKey] : null;
            }
        }

        public virtual Sprite Add(BackgroundImageClass image)
        {
            var imageKey = new ImageMetadata(image);
            if (spriteList.ContainsKey(imageKey))
                return spriteList[imageKey];
            var currentPositionToReturn = SpriteContainer.Width;
            SpriteContainer.AddImage(image);
            if (SpriteContainer.Size >= config.SpriteSizeLimit)
                Flush();
            var sprite = new Sprite(currentPositionToReturn, GetSpriteUrl());
            spriteList.Add(imageKey, sprite);
            return sprite;
        }

        public virtual void Flush()
        {
            if(SpriteContainer.Size > 0)
            {
                using (var spriteWriter = spriteWriterFactory.CreateWriter(SpriteContainer.Width, SpriteContainer.Height))
                {
                    foreach (var image in SpriteContainer)
                    {
                        spriteWriter.WriteImage(image);
                    }

                    spriteWriter.Save(GetSpriteUrl(), "image/png");
                }
            }
            SpriteContainer.Dispose();
            ++spriteIndex;
            SpriteContainer = new SpriteContainer(webClientWrapper);
            return;
        }

        public Guid SpritedCssKey { get; set; }

        private string GetSpriteUrl()
        {
            if (SpritedCssKey == Guid.Empty)
                throw new InvalidOperationException("The SpritedCssKey must be set before using the SprieManager.");
            return uriBuilder.BuildSpriteUrl(SpritedCssKey, spriteIndex);
        }

        private struct ImageMetadata
        {
            public ImageMetadata(BackgroundImageClass image) : this()
            {
                Url = image.ImageUrl;
                Width = image.Width ?? 0;
                Height = image.Height ?? 0;
                XOffset = image.XOffset.Offset;
            }

            public int Width { get; private set; }
            public int Height { get; private set; }
            public int XOffset { get; private set; }
            public string Url { get; private set; }
        }
    }
}