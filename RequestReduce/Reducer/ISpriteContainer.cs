using System;
using System.Collections.Generic;
using System.Drawing;

namespace RequestReduce.Reducer
{
    public interface ISpriteContainer : IEnumerable<OrderedImage>, IDisposable
    {
        void AddImage (BackgroundImageClass image, Sprite sprite);
        int Size { get; }
        int Colors { get; }
        int Width { get; }
        int Height { get; }
    }
}