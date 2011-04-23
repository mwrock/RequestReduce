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

        public SpriteManager(IWebClientWrapper webClientWrapper, IConfigurationWrapper configWrapper, IFileWrapper fileWrapper, HttpContextBase httpContext)
        {
            this.webClientWrapper = webClientWrapper;
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
            int size;
            var bitmap = webClientWrapper.DownloadImage(imageUrl, out size);
            SpriteContainer.Size += size;
            SpriteContainer.Images.Add(bitmap);
            var currentPositionToReturn = SpriteContainer.Width;
            var currentUrlToReturn = SpriteContainer.Url;
            SpriteContainer.Width += bitmap.Width;
            if(SpriteContainer.Height < bitmap.Height) SpriteContainer.Height= bitmap.Height;
            if (SpriteContainer.Size >= configWrapper.SpriteSizeLimit)
                Flush();
            return new Sprite(currentPositionToReturn, currentUrlToReturn);
        }

        public virtual void Flush()
        {
            using (Bitmap sprite = new Bitmap(SpriteContainer.Width, SpriteContainer.Height))
            using (Graphics drawingSurface = Graphics.FromImage(sprite))
            {
                drawingSurface.Clear(Color.Transparent);

                var xOffset = 0;
                foreach (var image in SpriteContainer.Images)
                {
                    drawingSurface.DrawImage(image, new Rectangle(xOffset, 0, image.Width, image.Height));
                    xOffset += image.Width;
                }

                using (var spriteEncoderParameters = new EncoderParameters(1))
                {
                    spriteEncoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, 90);
                    using (var stream = fileWrapper.OpenStream(httpContext.Server.MapPath(SpriteContainer.Url)))
                    {
                        sprite.Save(stream, ImageCodecInfo.GetImageEncoders().First(x => x.MimeType == "image/png"), spriteEncoderParameters);
                    }
                }
            }
            return;
        }
    }
}