using System.Collections.Generic;
using System.Drawing;

namespace RequestReduce.Facts
{
    public class BitmapPixelComparer : IEqualityComparer<Bitmap>
    {
        private bool whiteIsTransparent = false;

        public BitmapPixelComparer()
        {
            
        }

        public BitmapPixelComparer(bool whiteIsTransparent)
        {
            this.whiteIsTransparent = whiteIsTransparent;
        }

        public bool Equals(Bitmap bitmap1, Bitmap bitmap2)
        {
            if (bitmap1.PhysicalDimension != bitmap2.PhysicalDimension)
                return false;

            for (var x = 0; x < bitmap1.Width; x++)
            {
                for (var y = 0; y < bitmap1.Height; y++)
                {
                    if (bitmap1.GetPixel(x, y) != bitmap2.GetPixel(x, y))
                    {
                        if (whiteIsTransparent && bitmap1.GetPixel(x, y).Name.Replace("ffffff", "0") == bitmap2.GetPixel(x, y).Name.Replace("ffffff", "0"))
                        {
                            continue;
                        }
                        return false;
                    }
                }
            }
            return true;
        }

        public int GetHashCode(Bitmap obj)
        {
            return obj.GetHashCode();
        }
    }
}