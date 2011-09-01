using System;
using System.Collections.Generic;
using System.Drawing;

namespace RequestReduce.Reducer
{
    public interface ISpriteContainer : IEnumerable<SpritedImage>, IDisposable
    {
        SpritedImage AddImage (BackgroundImageClass image);
        int Size { get; }
        int Colors { get; }
        int Width { get; }
        int Height { get; }
    }
}