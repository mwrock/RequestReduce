using System;
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
        private static readonly Regex shortcutOffsetPattern = new Regex(@"background:[\s\w]*url[\s]*\([^\)]*\)[\s]+(?:[\-a-z]+[\s]+)?(?<offset1>right|left|bottom|top|center|(?:\-?\d+(?:%|px|in|cm|mm|em|ex|pt|pc)?))[\s;](?:(?<offset2>right|left|bottom|top|center|(?:\-?\d+(?:%|px|in|cm|mm|em|ex|pt|pc)?))[\s;])?", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
        private static readonly Regex repeatPattern = new Regex(@"\b((x-)|(y-)|(no-))?repeat\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex widthPattern = new Regex(@"\b(max-)?width:[\s]*(?<width>[0-9]+)(px)?[\s]*;", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex heightPattern = new Regex(@"\b(max-)?height:[\s]*(?<height>[0-9]+)(px)?[\s]*;", RegexOptions.Compiled | RegexOptions.IgnoreCase);

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

            var heightMatch = heightPattern.Matches(originalClassString);
            if (heightMatch.Count > 0)
                Height = Int32.Parse(heightMatch[heightMatch.Count - 1].Groups["height"].Value);
            var offsetMatch = shortcutOffsetPattern.Match(originalClassString);
            if (offsetMatch.Success)
            {
                var offset1 = offsetMatch.Groups["offset1"].Value;
                var offset2 = offsetMatch.Groups["offset2"].Value;
                if(offset1.ToLower() == "top" || offset1.ToLower() == "bottom")
                    YOffset = ParseStringOffset(offset1);
                else
                    XOffset = ",right,left,center".IndexOf(offset1.ToLower()) > 0
                                  ? ParseStringOffset(offset1)
                                  : ParseNumericOffset(offset1);
                if (offset2.ToLower() == "right" || offset2.ToLower() == "left")
                    XOffset = ParseStringOffset(offset2);
                else
                    YOffset = ",top,bottom,center".IndexOf(offset2.ToLower()) > 0
                                  ? ParseStringOffset(offset2)
                                  : ParseNumericOffset(offset2);
            }
        }

        private Position ParseStringOffset(string offsetString)
        {
            var offset = new Position();
            offset.PositionMode = PositionMode.Direction;
            Direction direction;
            Enum.TryParse(offsetString, true, out direction);
            offset.Direction = direction;
            return offset;
        }

        private Position ParseNumericOffset(string offsetString)
        {
            var offset = new Position();
            if (offsetString.Length > 2 && "|in|cm|mm|em|ex|pt|pc".IndexOf(offsetString.Substring(offsetString.Length-2,2).ToLower()) > -1)
                return offset;
            var trim = 0;
            if (offsetString.EndsWith("%"))
            {
                trim = 1;
                offset.PositionMode = PositionMode.Percent;
            }
            else 
                offset.PositionMode = PositionMode.Unit;
            if (offsetString.ToLower().EndsWith("px"))
                trim = 2;
            int units;
            Int32.TryParse(offsetString.Substring(0, offsetString.Length - trim), out units);
            offset.Offset = units;
            return offset;
        }

        public string OriginalClassString { get; set; }
        public string ImageUrl { get; set; }
        public RepeatStyle Repeat { get; set; }
        public Position XOffset { get; set; }
        public Position YOffset { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }

        public string Render()
        {
            return null;
        }
    }
}
