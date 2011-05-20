using System;
using System.Collections.Generic;
using System.Drawing;

namespace RequestReduce.Reducer
{
    public interface ISpriteContainer : IEnumerable<Bitmap>, IDisposable
    {
        void AddImage (BackgroungImageClass image);
        string Url { get; set; }
        int Size { get; }
        int Width { get; }
        int Height { get; }
    }
}