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
        private IDictionary<ImageMetadata, Sprite> spriteList = new Dictionary<ImageMetadata, Sprite>();

        public SpriteManager(IWebClientWrapper webClientWrapper, IRRConfiguration config, ISpriteWriterFactory spriteWriterFactory)
        {
            this.webClientWrapper = webClientWrapper;
            this.spriteWriterFactory = spriteWriterFactory;
            this.config = config;
            SpriteContainer = new SpriteContainer(config, webClientWrapper);
        }

        public virtual Sprite this[BackgroungImageClass image]
        {
            get
            {
                var imageKey = new ImageMetadata(image);
                return spriteList.ContainsKey(imageKey) ? spriteList[imageKey] : null;
            }
        }

        public virtual Sprite Add(BackgroungImageClass image)
        {
            var imageKey = new ImageMetadata(image);
            if (spriteList.ContainsKey(imageKey))
                return spriteList[imageKey];
            var currentPositionToReturn = SpriteContainer.Width;
            var currentUrlToReturn = SpriteContainer.Url;
            SpriteContainer.AddImage(image);
            if (SpriteContainer.Size >= config.SpriteSizeLimit)
                Flush();
            var sprite = new Sprite(currentPositionToReturn, currentUrlToReturn);
            spriteList.Add(imageKey, sprite);
            return sprite;
        }

        public virtual void Flush()
        {
            using (var spriteWriter = spriteWriterFactory.CreateWriter(SpriteContainer.Width, SpriteContainer.Height))
            {
                foreach (var image in SpriteContainer)
                {
                    spriteWriter.WriteImage(image);
                }

                spriteWriter.Save(SpriteContainer.FilePath, "image/png");
            }
            SpriteContainer.Dispose();
            SpriteContainer = new SpriteContainer(config, webClientWrapper);
            return;
        }

        private struct ImageMetadata
        {
            public ImageMetadata(BackgroungImageClass image) : this()
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