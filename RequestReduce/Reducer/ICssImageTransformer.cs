using System.Collections.Generic;

namespace RequestReduce.Reducer
{
    public interface ICssImageTransformer
    {
        IEnumerable<BackgroundImageClass> ExtractImageUrls(ref string cssContent, string cssUrl);
        string InjectSprite(string originalCss, SpritedImage image);
    }
}