using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace RequestReduce.Reducer
{
    public class CssImageTransformer : ICssImageTransformer
    {
        private static readonly Regex classPattern = new Regex("\\{[^\\}]+\\}", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);

        public IEnumerable<BackgroundImageClass> ExtractImageUrls(ref string cssContent, string cssUrl)
        {
            var urls = new List<BackgroundImageClass>();
            foreach (var classMatch in classPattern.Matches(cssContent))
            {
                var imageClass = new BackgroundImageClass(classMatch.ToString(), cssUrl);
                if(imageClass.OriginalImageUrl != null)
                    cssContent = cssContent.Replace(classMatch.ToString(), imageClass.OriginalClassString);
                if (imageClass.ImageUrl != null)
                {
                    if (imageClass.Width > 0 
                        && imageClass.Repeat == RepeatStyle.NoRepeat 
                        && ((imageClass.XOffset.PositionMode == PositionMode.Direction && imageClass.XOffset.Direction == Direction.Left)
                            || (imageClass.XOffset.PositionMode != PositionMode.Direction && imageClass.XOffset.Offset <= 0)) 
                        && ((imageClass.YOffset.PositionMode == PositionMode.Direction && imageClass.YOffset.Direction == Direction.Top)
                            || (imageClass.YOffset.PositionMode == PositionMode.Percent && imageClass.YOffset.Offset == 0) 
                            || ((imageClass.YOffset.PositionMode == PositionMode.Unit) &&  (imageClass.YOffset.Offset >= 0 || imageClass.Height > 0))))
                                urls.Add(imageClass);
                }
            }
            return urls;
        }

        public string InjectSprite(string originalCss, SpritedImage image)
        {
            return originalCss.Replace(image.CssClass.OriginalClassString, image.Render());
        }
    }
}