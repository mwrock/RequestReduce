using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using RequestReduce.Utilities;
using RequestReduce.Configuration;

namespace RequestReduce.Reducer
{
    public class SpriteContainer : ISpriteContainer
    {
        protected readonly IList<SpritedImage> images = new List<SpritedImage>();
        private readonly IWebClientWrapper webClientWrapper;
        private readonly IRRConfiguration rrConfiguration;
        private readonly HashSet<int> uniqueColors = new HashSet<int>();

        public SpriteContainer(IWebClientWrapper webClientWrapper, IRRConfiguration rrConfiguration)
        {
            this.webClientWrapper = webClientWrapper;
            this.rrConfiguration = rrConfiguration;
        }

        public void AddImage (SpritedImage image)
        {
            images.Add(image);
        }

        public SpritedImage AddImage (BackgroundImageClass image)
        {
            var imageBytes = webClientWrapper.DownloadBytes(image.ImageUrl);
            Bitmap bitmap;

            using (var originalBitmap = new Bitmap(new MemoryStream(imageBytes)))
            {
                using (var writer = new SpriteWriter(image.Width ?? originalBitmap.Width, image.Height ?? originalBitmap.Height))
                {
                    var x = image.XOffset.Offset < 0 ? Math.Abs(image.XOffset.Offset) : 0;
                    var y = image.YOffset.Offset < 0 ? Math.Abs(image.YOffset.Offset) : 0;
                    var width = image.Width ?? originalBitmap.Width;
                    if (width + x > originalBitmap.Width)   
                        width = originalBitmap.Width - x;
                    var height = image.Height ?? originalBitmap.Height;
                    if (height + y > originalBitmap.Height)
                        height = originalBitmap.Height - y;
                    var offsetX = 0;
                    var offsetY = 0;
                    if (image.XOffset.Direction == Direction.Right)
                    {
                        if (originalBitmap.Width > width)
                            x = originalBitmap.Width - width;
                        else
                            offsetX = (image.Width ?? 0) - originalBitmap.Width;
                    }
                    else if (image.XOffset.Direction == Direction.Center && image.XOffset.PositionMode == PositionMode.Direction)
                    {
                        if (originalBitmap.Width > width)
                            x = (originalBitmap.Width - width)/2;
                        else
                            offsetX = ((image.Width ?? 0) - originalBitmap.Width)/2;
                    }
                    if (image.YOffset.Direction == Direction.Bottom)
                    {
                        if (originalBitmap.Height > height)
                            y = originalBitmap.Height - height;
                        else
                            offsetY = (image.Height ?? 0) - originalBitmap.Height;
                    }
                    else if (image.YOffset.Direction == Direction.Center && image.YOffset.PositionMode == PositionMode.Direction)
                    {
                        if (originalBitmap.Height > height)
                            y = (originalBitmap.Height - height) / 2;
                        else
                            offsetY = ((image.Height ?? 0) - originalBitmap.Height) / 2;
                    }

                    try
                    {
                        using (var bm = originalBitmap.Clone(new Rectangle(x, y, width, height), originalBitmap.PixelFormat))
                        {
                            writer.WriteImage(bm, offsetX, offsetY);
                        }
                    }
                    catch (OutOfMemoryException)
                    {
                        throw new InvalidOperationException(string.Format("Unable to Add {0} to Sprite.", image.ImageUrl));
                    }
                    bitmap = writer.SpriteImage;
                    if ((originalBitmap.Width * originalBitmap.Height) > (bitmap.Width * bitmap.Height))
                        Size += writer.GetBytes("image/png").Length;
                    else
                        Size += imageBytes.Length;
                }
            }
            var avgColor = rrConfiguration.IsFullTrust ? GetColors(bitmap) : 0;
            var spritedImage = new SpritedImage(avgColor, image, bitmap);
            images.Add(spritedImage);
            Width += bitmap.Width + 1;
            if (Height < bitmap.Height) Height = bitmap.Height;
            return spritedImage;
        }


        private int GetColors(Bitmap bitmap)
        {
            long totalArgb = 0;
            var total = 0;
            var data = bitmap.LockBits(Rectangle.FromLTRB(0, 0, bitmap.Width, bitmap.Height),
                                            ImageLockMode.ReadOnly, bitmap.PixelFormat);
            try
            {
                var byteLength = data.Stride < 0 ? -data.Stride : data.Stride;
                var buffer = new Byte[byteLength * bitmap.Height];
                var offset = 0;
                Marshal.Copy(data.Scan0, buffer, 0, buffer.Length);
                for (var y = 0; y < bitmap.Height; y++)
                {
                    for (var x = 0; x < bitmap.Width; x++)
                    {
                        var argb = BitConverter.ToInt32(buffer, offset);
                        uniqueColors.Add(argb);
                        totalArgb += argb;
                        ++total;
                        offset += 4;
                    }
                }

                return (int)(totalArgb / total);
            }
            finally
            {
                bitmap.UnlockBits(data);
            }
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
    }
}
