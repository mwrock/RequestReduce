using System.Collections.Generic;
using System.Drawing;

namespace RequestReduce.Reducer
{
    public interface ISpriteContainer : IEnumerable<Bitmap>
    {
        void AddImage (byte[] image);
        string Url { get; set; }
        int Size { get; }
        int Width { get; }
        int Height { get; }
        IEnumerator<Bitmap> GetEnumerator();
    }
}