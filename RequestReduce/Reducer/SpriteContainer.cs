using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using RequestReduce.Utilities;

namespace RequestReduce.Reducer
{
    public class SpriteContainer : ISpriteContainer
    {
        protected readonly IList<SpritedImage> images = new List<SpritedImage>();
        private readonly IWebClientWrapper webClientWrapper;
        private readonly HashSet<Color> uniqueColors = new HashSet<Color>();

        public SpriteContainer(IWebClientWrapper webClientWrapper)
        {
            this.webClientWrapper = webClientWrapper;
        }

        public SpritedImage AddImage (BackgroundImageClass image)
        {
            var imageBytes = webClientWrapper.DownloadBytes(image.ImageUrl);
            Bitmap bitmap;
            using (var originalBitmap = new Bitmap(new MemoryStream(imageBytes)))
            {
                using (var writer = new SpriteWriter(image.Width ?? originalBitmap.Width, image.Height ?? originalBitmap.Height))
                {
                    var width = image.Width ?? originalBitmap.Width;
                    if (width > originalBitmap.Width)
                        width = originalBitmap.Width;
                    var height = image.Height ?? originalBitmap.Height;
                    if (height > originalBitmap.Height)
                        height = originalBitmap.Height;
                    var x = image.XOffset.Offset < 0 ? Math.Abs(image.XOffset.Offset) : 0;
                    var y = image.YOffset.Offset < 0 ? Math.Abs(image.YOffset.Offset) : 0;

                    writer.WriteImage(originalBitmap.Clone(new Rectangle(x, y, width, height), originalBitmap.PixelFormat));
                    bitmap = writer.SpriteImage;
                    if ((originalBitmap.Width * originalBitmap.Height) > (bitmap.Width * bitmap.Height))
                        Size += writer.GetBytes("image/png").Length;
                    else
                        Size += imageBytes.Length;
                }
            }
            var avgColor = GetColors(bitmap);
            var spritedImage = new SpritedImage(avgColor, image, bitmap);
            images.Add(spritedImage);
            Width += bitmap.Width + 1;
            if (Height < bitmap.Height) Height = bitmap.Height;
            return spritedImage;
        }

        private int GetColors(Bitmap bitmap)
        {
            long argb = 0;
            var total = 0;
            for(var x = 0; x < bitmap.Width;x++)
            {
                for(var y=0; y < bitmap.Height; y++)
                {
                    Color color = bitmap.GetPixel(x, y);
                    uniqueColors.Add(bitmap.GetPixel(x, y));
                    argb += color.ToArgb();
                    ++total;
                }
            }

            return (int)(argb / total);
        }

        public int Size { get; private set; }
        public int Colors
        {
            get { return uniqueColors.Count; }
        }

        public int Width { get; private set; }
        public int Height { get; private set; }
        public IEnumerator<SpritedImage> GetEnumerator()
        {
            return images.OrderBy(x => x.AverageColor).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Dispose()
        {
            images.ToList().ForEach(x => x.Image.Dispose());
        }
    }
}
