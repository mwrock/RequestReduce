using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;
using RequestReduce.Configuration;
using RequestReduce.Utilities;

namespace RequestReduce.Reducer
{
    public class SpriteManager : ISpriteManager
    {
        protected SpriteContainer SpriteContainer = new SpriteContainer();
        private IWebClientWrapper webClientWrapper = null;
        private IConfigurationWrapper configWrapper = null;
        private readonly IFileWrapper fileWrapper;
        private readonly HttpContextBase httpContext;
        private readonly ISpriteWriterFactory spriteWriterFactory;

        public SpriteManager(IWebClientWrapper webClientWrapper, IConfigurationWrapper configWrapper, IFileWrapper fileWrapper, HttpContextBase httpContext, ISpriteWriterFactory spriteWriterFactory)
        {
            this.webClientWrapper = webClientWrapper;
            this.spriteWriterFactory = spriteWriterFactory;
            this.httpContext = httpContext;
            this.fileWrapper = fileWrapper;
            this.configWrapper = configWrapper;
            SpriteContainer.Url = string.Format("{0}/{1}.png", configWrapper.SpriteDirectory, Guid.NewGuid().ToString());
        }

        public virtual bool Contains(string imageUrl)
        {
            throw new NotImplementedException();
        }

        public virtual Sprite this[string imageUrl]
        {
            get { throw new NotImplementedException(); }
        }

        public virtual Sprite Add(string imageUrl)
        {
            var currentPositionToReturn = SpriteContainer.Width;
            var currentUrlToReturn = SpriteContainer.Url;
            SpriteContainer.AddImage(imageUrl);
            if (SpriteContainer.Size >= configWrapper.SpriteSizeLimit)
                Flush();
            return new Sprite(currentPositionToReturn, currentUrlToReturn);
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
            return;
        }
    }
}