using System.Collections.Generic;
using System.Drawing;

namespace RequestReduce.Reducer
{
    public class SpriteContainer
    {
        public SpriteContainer()
        {
            Images = new List<Bitmap>();
        }

        public IList<Bitmap> Images { get; set; }
        public string Url { get; set; }
        public int Size { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }
}
