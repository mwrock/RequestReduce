using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using RequestReduce.Store;
using RequestReduce.Utilities;

namespace RequestReduce.Reducer
{
    public class SpriteWriter : ISpriteWriter
    {
        private readonly IStore store;
        public Bitmap SpriteImage { get; private set; }
        private readonly Graphics drawingSurface = null;

        public SpriteWriter(int surfaceWidth, int surfaceHeight, IStore store)
        {
            this.store = store;
            SpriteImage = new Bitmap(surfaceWidth, surfaceHeight);
            drawingSurface = Graphics.FromImage(SpriteImage);
            drawingSurface.Clear(Color.Transparent);
        }

        public void WriteImage(Bitmap image)
        {
            drawingSurface.DrawImage(image, new Rectangle(OffsetWidth, 0, image.Width, image.Height));
            OffsetWidth += image.Width;
        }

        public void Save(string url, string mimeType)
        {
            using (var spriteEncoderParameters = new EncoderParameters(1))
            {
                spriteEncoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, 90);
                using (var stream = store.OpenStream(url))
                {
                    SpriteImage.Save(stream, ImageCodecInfo.GetImageEncoders().First(x => x.MimeType == mimeType), spriteEncoderParameters);
                }
            }
        }

        public int OffsetWidth { get; private set; }

        public void Dispose()
        {
            if (drawingSurface != null)
                drawingSurface.Dispose();
        }
    }
}
