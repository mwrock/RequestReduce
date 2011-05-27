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

    public class BackgroundImageClass
    {
        private static readonly Regex imageUrlPattern = new Regex(@"background(-image)?:[\s\w#]*url[\s]*\([\s]*(?<url>[^\)]*)[\s]*\)[^;]*;", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
        private static readonly Regex offsetPattern = new Regex(@"background(?:-position)?:(?:[^;]*?)(?<offset1>right|left|bottom|top|center|(?:\-?\d+(?:%|px|in|cm|mm|em|ex|pt|pc)?))[\s;](?:(?<offset2>right|left|bottom|top|center|(?:\-?\d+(?:%|px|in|cm|mm|em|ex|pt|pc)?))[\s;])?", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
        private static readonly Regex repeatPattern = new Regex(@"\b((x-)|(y-)|(no-))?repeat\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex widthPattern = new Regex(@"\b(max-)?width:[\s]*(?<width>[0-9]+)(px)?[\s]*;", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex heightPattern = new Regex(@"\b(max-)?height:[\s]*(?<height>[0-9]+)(px)?[\s]*;", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public BackgroundImageClass(string originalClassString)
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
            var offsetMatches = offsetPattern.Matches(originalClassString);
            if (offsetMatches.Count > 0)
            {
                foreach (Match offsetMatch in offsetMatches)
                    SetOffsets(offsetMatch);
            }
        }

        private void SetOffsets(Match offsetMatch)
        {
            var offset1 = offsetMatch.Groups["offset1"].Value;
            var offset2 = offsetMatch.Groups["offset2"].Value;
            var flip = false;
            if (offset1.ToLower() == "top" || offset1.ToLower() == "bottom" || offset2.ToLower() == "right" || offset2.ToLower() == "left")
                flip = true;
            var offset1Position = ",top,bottom,right,left,center".IndexOf(offset1.ToLower()) > 0
                                      ? ParseStringOffset(offset1)
                                      : ParseNumericOffset(offset1);
            var offset2Position = ",top,bottom,right,left,center".IndexOf(offset2.ToLower()) > 0
                                      ? ParseStringOffset(offset2)
                                      : ParseNumericOffset(offset2);
            if (flip)
            {
                if(offset2Position.PositionMode != PositionMode.Percent || offset2Position.Offset > 0) XOffset = offset2Position;
                if(offset1Position.PositionMode != PositionMode.Percent || offset1Position.Offset > 0) YOffset = offset1Position;
            }
            else
            {
                if(offset1Position.PositionMode != PositionMode.Percent || offset1Position.Offset > 0) XOffset = offset1Position;
                if(offset2Position.PositionMode != PositionMode.Percent || offset2Position.Offset > 0) YOffset = offset2Position;
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
            if (string.IsNullOrEmpty(offsetString))
                return offset;
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

        public string Render(Sprite sprite)
        {
            var newClass = OriginalClassString.ToLower().Replace(ImageUrl.ToLower(), sprite.Url);
            var yOffset = YOffset.Direction.ToString();
            if (YOffset.PositionMode != PositionMode.Direction)
                yOffset = "0";
            if (YOffset.PositionMode == PositionMode.Unit && YOffset.Offset > 0)
                yOffset = string.Format("{0}px", YOffset.Offset);

            newClass = newClass.Replace("}",
                                        string.Format(";background-position: -{0}px {1};}}",
                                                      sprite.Position, yOffset));
            return newClass;
        }
    }
}
