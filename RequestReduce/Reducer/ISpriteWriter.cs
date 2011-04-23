using System;
using System.Drawing;

namespace RequestReduce.Reducer
{
    public interface ISpriteWriter : IDisposable
    {
        Bitmap SpriteImage { get; }
        int OffsetWidth { get; }
        void WriteImage(Bitmap image);
        void Save(string fileName, string mimeType);
    }
}