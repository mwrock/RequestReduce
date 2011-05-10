using System.Collections.Generic;

namespace RequestReduce.Reducer
{
    public interface ICssImageTransformer
    {
        IEnumerable<BackgroungImageClass> ExtractImageUrls(string cssContent);
        string InjectSprite(string originalCss, BackgroungImageClass imageUrl, Sprite sprite);
    }
}