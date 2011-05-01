using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace RequestReduce.Reducer
{
    public enum RepeatStyle
    {
        Repeat,
        NoRepeat,
        XRepeat,
        YRepeat
    }

    public enum PositionMode
    {
        Percent,
        Unit,
        Direction,
    }

    public enum Direction
    {
        Center,
        Left,
        Right,
        Top,
        Bottom
    }

    public class BackgroungImageClass
    {
        private static readonly Regex imageUrlPattern = new Regex(@"background(-image)?:[\s\w]*url[\s]*\([\s]*(?<url>[^\)]*)[\s]*\)[^;]*;", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
        private static readonly Regex repeatPattern = new Regex(@"\b((x-)|(y-)|(no-))?repeat\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex widthPattern = new Regex(@"\b(max-)?width:[\s]*(?<width>[0-9]+)(px)?[\s]*;", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public BackgroungImageClass(string originalClassString)
        {
            OriginalClassString = originalClassString;
            var match = imageUrlPattern.Match(originalClassString);
            if (match.Success)
                ImageUrl = match.Groups["url"].Value.Replace("'", "").Replace("\"", "");
            var repeatMatch = repeatPattern.Matches(originalClassString);
            if(repeatMatch.Count > 0)
            {
                RepeatStyle rep;
                Enum.TryParse(repeatMatch[repeatMatch.Count-1].Value.Replace("-",""), true, out rep);
                Repeat = rep;
            }
            var widthMatch = widthPattern.Matches(originalClassString);
            if (widthMatch.Count > 0)
                Width = Int32.Parse(widthMatch[widthMatch.Count - 1].Groups["width"].Value);
        }

        public string OriginalClassString { get; set; }
        public string ImageUrl { get; set; }
        public RepeatStyle Repeat { get; set; }
        public Position XOffset { get; set; }
        public Position YOffset { get; set; }
        public int? Width { get; set; }

        public string Render()
        {
            return null;
        }
    }
}
