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
        private readonly IList<Bitmap> images = new List<Bitmap>();
        private readonly IWebClientWrapper webClientWrapper;

        public SpriteContainer(IWebClientWrapper webClientWrapper)
        {
            this.webClientWrapper = webClientWrapper;
        }

        public void AddImage (BackgroundImageClass image)
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
                }
            }
            images.Add(bitmap);
            Size += imageBytes.Length;
            Width += bitmap.Width;
            if (Height < bitmap.Height) Height = bitmap.Height;
        }

        public int Size { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public IEnumerator<Bitmap> GetEnumerator()
        {
            return images.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Dispose()
        {
            images.ToList().ForEach(x => x.Dispose());
        }
    }
}
