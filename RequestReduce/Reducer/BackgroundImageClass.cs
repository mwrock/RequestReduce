using System;
using System.Linq;
using System.Text.RegularExpressions;
using RequestReduce.Utilities;

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
        private static readonly Regex imageUrlPattern = new Regex(@"background(-image)?:[\s\w#]*url[\s]*\([\s]*(?<url>[^\)]*)[\s]*\)[^;}]*", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
        private static readonly Regex offsetPattern = new Regex(@"background(?:-position)?:(?:[^;]*?)(?<offset1>right|left|bottom|top|center|(?:\-?\d+(?:%|px|in|cm|mm|em|ex|pt|pc)?))[\s;](?:(?<offset2>right|left|bottom|top|center|(?:\-?\d+(?:%|px|in|cm|mm|em|ex|pt|pc)?))[\s;])?", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
        private static readonly Regex repeatPattern = new Regex(@"\b((x-)|(y-)|(no-))?repeat\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex widthPattern = new Regex(@"\b(max-)?width:[\s]*(?<width>[0-9]+)(px)?[\s]*[;}]", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex heightPattern = new Regex(@"\b(max-)?height:[\s]*(?<height>[0-9]+)(px)?[\s]*[;}]", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex paddingPattern = new Regex(@"padding(?<side>-(left|top|right|bottom))?:[\s]*(?<pad1>(?:\d+(?:%|px|in|cm|mm|em|ex|pt|pc)?))[\s;]((?<pad2>(?:\d+(?:%|px|in|cm|mm|em|ex|pt|pc)?))[\s;])?((?<pad3>(?:\d+(?:%|px|in|cm|mm|em|ex|pt|pc)?))[\s;])?((?<pad4>(?:\d+(?:%|px|in|cm|mm|em|ex|pt|pc)?))[\s;])?", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.ExplicitCapture);

        public BackgroundImageClass(string originalClassString, string parentCssUrl)
        {
            OriginalClassString = originalClassString;
            var match = imageUrlPattern.Match(originalClassString);
            if (match.Success)
            {
                OriginalImageUrl = match.Groups["url"].Value.Replace("'", "").Replace("\"", "");
                if (OriginalImageUrl.Length > 0)
                {
                    ImageUrl = RelativeToAbsoluteUtility.ToAbsolute(parentCssUrl, OriginalImageUrl);
                    OriginalClassString = OriginalClassString.Replace(OriginalImageUrl, ImageUrl);
                }
            }
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

            if(Width != null || Height != null)
            {
                var paddingMatches = paddingPattern.Matches(originalClassString);
                if (paddingMatches.Count > 0)
                {
                    var padVals = new int[4];
                    foreach (var pads in from Match paddingMatch in paddingMatches select GetPadding(paddingMatch))
                    {
                        if (pads[0] != null)
                            padVals[0] = (int)pads[0];
                        if (pads[1] != null)
                            padVals[1] = (int)pads[1];
                        if (pads[2] != null)
                            padVals[2] = (int)pads[2];
                        if (pads[3] != null)
                            padVals[3] = (int)pads[3];
                    }
                    if(Width != null)
                    {
                        if (padVals[1] < 0 || padVals[3] < 0)
                            Width = null;
                        else
                            Width += (padVals[1] + padVals[3]);
                    }
                    if (Height != null)
                    {
                        if (padVals[0] < 0 || padVals[2] < 0)
                            Height = null;
                        else
                            Height += (padVals[0] + padVals[2]);
                    }
                }
            }

            var offsetMatches = offsetPattern.Matches(originalClassString);
            if (offsetMatches.Count > 0)
            {
                foreach (Match offsetMatch in offsetMatches)
                    SetOffsets(offsetMatch);
            }
        }

        private int?[] GetPadding(Match paddingMatch)
        {
            var padVals = new int?[4];
            var side = paddingMatch.Groups["side"].Value.ToLower();
            var pad1 = paddingMatch.Groups["pad1"].Value;
            var pad2 = paddingMatch.Groups["pad2"].Value;
            var pad3 = paddingMatch.Groups["pad3"].Value;
            var pad4 = paddingMatch.Groups["pad4"].Value;

            if (side == "-left")
                padVals[3] = calcPadPixels(pad1, Width);
            else if (side == "-right")
                padVals[1] = calcPadPixels(pad1, Width);
            else if (side == "-top")
                padVals[0] = calcPadPixels(pad1, Height);
            else if (side == "-bottom")
                padVals[2] = calcPadPixels(pad1, Height);
            else
            {
                var groupCount = paddingMatch.Groups.Cast<Group>().Where(x => x.Length > 0).Count();
                switch (groupCount-1)
                {
                    case 1:
                        padVals[0] = calcPadPixels(pad1, Height);
                        padVals[1] = calcPadPixels(pad1, Width);
                        padVals[2] = calcPadPixels(pad1, Height);
                        padVals[3] = calcPadPixels(pad1, Width);
                        break;
                    case 2:
                        padVals[0] = calcPadPixels(pad1, Height);
                        padVals[1] = calcPadPixels(pad2, Width);
                        padVals[2] = calcPadPixels(pad1, Height);
                        padVals[3] = calcPadPixels(pad2, Width);
                        break;
                    case 3:
                        padVals[0] = calcPadPixels(pad1, Height);
                        padVals[1] = calcPadPixels(pad2, Width);
                        padVals[2] = calcPadPixels(pad3, Height);
                        padVals[3] = calcPadPixels(pad2, Width);
                        break;
                    case 4:
                        padVals[0] = calcPadPixels(pad1, Height);
                        padVals[1] = calcPadPixels(pad2, Width);
                        padVals[2] = calcPadPixels(pad3, Height);
                        padVals[3] = calcPadPixels(pad4, Width);
                        break;
                }
            }
            return padVals;
        }

        private int calcPadPixels(string pad, int? pixels)
        {
            var result = 0;
            if (pixels == null) return result;

            pad = pad.ToLower();
            if (pad.EndsWith("%"))
            {
                int.TryParse(pad.Replace("%", ""), out result);
                result = (int)((result/100f)*(int)pixels);
            }
            else if (pad.EndsWith("px"))
                int.TryParse(pad.Replace("px", ""), out result);
            else
                if(!int.TryParse(pad, out result))
                    result = -1;

            return result;
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
            var offset = new Position {PositionMode = PositionMode.Direction};
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
        public string OriginalImageUrl { get; set; }
        public string ImageUrl { get; set; }
        public RepeatStyle Repeat { get; set; }
        public Position XOffset { get; set; }
        public Position YOffset { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
    }
}
