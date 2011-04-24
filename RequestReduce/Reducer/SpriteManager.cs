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
        private IConfigurationWrapper configWrapper = null;
        private readonly HttpContextBase httpContext;
        private readonly ISpriteWriterFactory spriteWriterFactory;
        private IDictionary<string, Sprite> spriteList = new Dictionary<string, Sprite>();

        public SpriteManager(IWebClientWrapper webClientWrapper, IConfigurationWrapper configWrapper, HttpContextBase httpContext, ISpriteWriterFactory spriteWriterFactory)
        {
            this.webClientWrapper = webClientWrapper;
            this.spriteWriterFactory = spriteWriterFactory;
            this.httpContext = httpContext;
            this.configWrapper = configWrapper;
            SpriteContainer = new SpriteContainer(configWrapper);
        }

        public virtual Sprite this[string imageUrl]
        {
            get { return spriteList.ContainsKey(imageUrl) ? spriteList[imageUrl] : null; }
        }

        public virtual Sprite Add(string imageUrl)
        {
            if (spriteList.ContainsKey(imageUrl))
                return spriteList[imageUrl];
            var currentPositionToReturn = SpriteContainer.Width;
            var currentUrlToReturn = SpriteContainer.Url;
            SpriteContainer.AddImage(webClientWrapper.DownloadBytes(imageUrl));
            if (SpriteContainer.Size >= configWrapper.SpriteSizeLimit)
                Flush();
            var sprite = new Sprite(currentPositionToReturn, currentUrlToReturn);
            spriteList.Add(imageUrl, sprite);
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

                spriteWriter.Save(httpContext.Server.MapPath(SpriteContainer.Url), "image/png");
            }
            SpriteContainer = new SpriteContainer(configWrapper);
            return;
        }
    }
}