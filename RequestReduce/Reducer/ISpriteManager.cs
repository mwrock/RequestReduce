using System;
using System.Collections.Generic;

namespace RequestReduce.Reducer
{
    public interface ISpriteManager : IEnumerable<SpritedImage>
    {
        void Add(BackgroundImageClass imageUrl);
        void Flush();
        Guid SpritedCssKey { get; set; }
    }
}