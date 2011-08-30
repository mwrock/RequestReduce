using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using RequestReduce.Configuration;
using RequestReduce.Utilities;

namespace RequestReduce.Reducer
{
    public class SpriteContainer : ISpriteContainer
    {
        private readonly IList<OrderedImage> images = new List<OrderedImage>();
        private readonly IWebClientWrapper webClientWrapper;
        private HashSet<Color> uniqueColors = new HashSet<Color>();


        public SpriteContainer(IWebClientWrapper webClientWrapper)
        {
            this.webClientWrapper = webClientWrapper;
        }

        public void AddImage (BackgroundImageClass image, Sprite sprite)
        {
            var imageBytes = webClientWrapper.DownloadBytes(image.ImageUrl);
            Bitmap bitmap = null;
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
            images.Add(new OrderedImage(){AverageColor = avgColor, Image = bitmap, Sprite = sprite});
            Width += bitmap.Width + 1;
            if (Height < bitmap.Height) Height = bitmap.Height;
        }

        private int GetColors(Bitmap bitmap)
        {
            long r = 0, g = 0, b = 0, total = 0;
            Color color = new Color();
            for(var x = 0; x < bitmap.Width;x++)
            {
                for(var y=0; y < bitmap.Height; y++)
                {
                    color = bitmap.GetPixel(x, y);
                    uniqueColors.Add(bitmap.GetPixel(x, y));
                    r += color.R;
                    g += color.G;
                    b += color.B;

                    ++total;
                }
            }

            r /= total;
            g /= total;
            b /= total;

            return Color.FromArgb((int) r, (int) g, (int) b).ToArgb();
        }

        public int Size { get; private set; }
        public int Colors
        {
            get { return uniqueColors.Count; }
        }

        public int Width { get; private set; }
        public int Height { get; private set; }
        public IEnumerator<OrderedImage> GetEnumerator()
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

    public class OrderedImage
    {
        public Bitmap Image { get; set; }
        public int AverageColor { get; set; }
        public Sprite Sprite { get; set; }
    }

}
