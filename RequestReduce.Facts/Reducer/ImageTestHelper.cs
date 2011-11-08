using System.Drawing;

namespace RequestReduce.Facts.Reducer
{
    public static class BitmapTestExtentions
    {
        public static Bitmap GraphicsImage(this Bitmap bitMap)
        {
            var image = new Bitmap(bitMap.Width, bitMap.Height);
            var drawingSurface = Graphics.FromImage(image);
            drawingSurface.Clear(Color.Transparent);
            drawingSurface.DrawImage(bitMap, new Rectangle(0, 0, bitMap.Width, bitMap.Height));
            return image;
        }
    }
}
