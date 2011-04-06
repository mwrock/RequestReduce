using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace RequestReduce.Reducer
{
    public class CssImageTransformer : ICssImageTransformer
    {
        private static readonly Regex classPattern = new Regex("\\{[^\\}]+\\}", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
        private static readonly Regex imageUrlPattern = new Regex(@"background(-image)?:[\s\w]*url[\s]*\([\s]*(?<url>[^\)]*)[\s]*\)(?:(?! repeat|(-[0-9])|( 0px))[^;])*;", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);

        public IEnumerable<string> ExtractImageUrls(string cssContent)
        {
            var urls = new List<string>();
            foreach (var classMatch in classPattern.Matches(cssContent))
            {
                var urlMatch = imageUrlPattern.Match(classMatch.ToString());
                if(urlMatch.Success)
                    urls.Add(urlMatch.Groups["url"].ToString().Replace("'", "").Replace("\"",""));
            }
            return urls;
        }

        public string InjectSprite(string imageUrl, Sprite sprite)
        {
            throw new NotImplementedException();
        }
    }
}