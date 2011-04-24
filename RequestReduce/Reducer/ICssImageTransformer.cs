using System.Collections.Generic;

namespace RequestReduce.Reducer
{
    public interface ICssImageTransformer
    {
        IEnumerable<string> ExtractImageUrls(string cssContent);
        string InjectSprite(string originalCss, string imageUrl, Sprite sprite);
    }
}