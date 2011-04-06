using System;
using System.Collections.Generic;

namespace RequestReduce.Reducer
{
    public class CssImageTransformer : ICssImageTransformer
    {
        public IEnumerable<string> ExtractImageUrls(string cssContent)
        {
            throw new NotImplementedException();
        }

        public string InjectSprite(string imageUrl, Sprite sprite)
        {
            throw new NotImplementedException();
        }
    }
}