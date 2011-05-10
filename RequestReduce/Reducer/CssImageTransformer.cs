using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace RequestReduce.Reducer
{
    public class CssImageTransformer : ICssImageTransformer
    {
        private static readonly Regex classPattern = new Regex("\\{[^\\}]+\\}", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);

        public IEnumerable<string> ExtractImageUrls(string cssContent)
        {
            var urls = new List<string>();
            foreach (var classMatch in classPattern.Matches(cssContent))
            {
                var imageClass = new BackgroungImageClass(classMatch.ToString());
                if (imageClass.ImageUrl != null)
                {
                    if (imageClass.Width > 0 && imageClass.Repeat == RepeatStyle.NoRepeat && ((imageClass.XOffset.PositionMode == PositionMode.Direction && imageClass.XOffset.Direction == Direction.Left) || (imageClass.XOffset.PositionMode != PositionMode.Direction && imageClass.XOffset.Offset <= 0)))
                        urls.Add(imageClass.ImageUrl);
                }
            }
            return urls;
        }

        public string InjectSprite(string originalCss, string imageUrl, Sprite sprite)
        {
            /*
            var urlClassPattern = new Regex(string.Format(@"\{[^\}]*{0}[^\}]*\}", imageUrl), RegexOptions.IgnoreCase | RegexOptions.Singleline);
            foreach (var classMatch in urlClassPattern.Matches(originalCss))
            {
                if (!positionUrlPattern.Match(classMatch.ToString()).Success && !repeatUrlPattern.Match(classMatch.ToString()).Success)
                    urls.Add(urlMatch.Groups["url"].ToString().Replace("'", "").Replace("\"", ""));
            }
            return originalCss.Replace(imageUrl, sprite.Url);
             * */
            return null;
        }
    }
}