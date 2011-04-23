using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using RequestReduce.Utilities;

namespace RequestReduce.Reducer
{
    public class SpriteWriter : ISpriteWriter
    {
        public Bitmap SpriteImage { get; private set; }
        private readonly Graphics drawingSurface = null;

        public SpriteWriter(int surfaceWidth, int surfaceHeight)
        {
            SpriteImage = new Bitmap(surfaceWidth, surfaceHeight);
            drawingSurface = Graphics.FromImage(SpriteImage);
            drawingSurface.Clear(Color.Transparent);
        }

        public void WriteImage(Bitmap image)
        {
            drawingSurface.DrawImage(image, new Rectangle(OffsetWidth, 0, image.Width, image.Height));
            OffsetWidth += image.Width;
        }

        public void Save(string fileName, string mimeType)
        {
            var fileWrapper = RRContainer.Current.GetInstance<IFileWrapper>();
            using (var spriteEncoderParameters = new EncoderParameters(1))
            {
                spriteEncoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, 90);
                using (var stream = fileWrapper.OpenStream(fileName))
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
            if (SpriteImage != null)
                SpriteImage.Dispose();
        }
    }
}
