using System;
using System.Collections.Generic;
using System.Drawing;

namespace RequestReduce.Reducer
{
    public interface ISpriteContainer : IEnumerable<Bitmap>, IDisposable
    {
        void AddImage (BackgroundImageClass image);
        string FilePath { get; set; }
        string Url { get; set; }
        int Size { get; }
        int Width { get; }
        int Height { get; }
    }
}