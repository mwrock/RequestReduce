using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace RequestReduce.Reducer
{
    public class CssImageTransformer : ICssImageTransformer
    {
        private readonly Regex classPattern = new Regex(@"(?<=\}|)[^\}]+\}", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);

        public IEnumerable<BackgroundImageClass> ExtractImageUrls(string cssContent)
        {
            var finalUrls = new List<BackgroundImageClass>();
            var draftUrls = new List<BackgroundImageClass>();
            foreach (var classMatch in classPattern.Matches(cssContent))
            {
                var imageClass = new BackgroundImageClass(classMatch.ToString());
                var analyzer = new CssSelectorAnalyzer();
                if (imageClass.PropertyCompletion == PropertyCompletion.HasNothing) continue;
                if (!IsComplete(imageClass) && ShouldFlatten(imageClass))
                {
                    var workList = new SortedDictionary<string,BackgroundImageClass>();
                    for (var n = draftUrls.Count - 1; n > -1; n--)
                    {
                        var selectors = draftUrls[n].Selector.Split(new [] {','});
                        var targetSelectors = imageClass.Selector.Split(new[] { ',' });
                        var counter = 0;
                        foreach (var selector in selectors)
                        {
                            foreach (var targetSelector in targetSelectors)
                            {
                                if(analyzer.IsInScopeOfTarget(targetSelector.Trim(),selector.Trim()))
                                {
                                    workList.Add(string.Format("{0}|{1}", selector.Trim(), counter++), draftUrls[n]);
                                    break;
                                }
                            }
                        }
                    }
                    for (var n = workList.Count - 1; n > -1 && !IsComplete(imageClass); n--)
                    {
                        var cls = draftUrls[n];
                        if((imageClass.PropertyCompletion & PropertyCompletion.HasYOffset) != PropertyCompletion.HasYOffset && (cls.PropertyCompletion & PropertyCompletion.HasYOffset) == PropertyCompletion.HasYOffset)
                        {
                            imageClass.YOffset = cls.YOffset;
                            imageClass.PropertyCompletion = imageClass.PropertyCompletion |
                                                            PropertyCompletion.HasYOffset;
                        }
                        if ((imageClass.PropertyCompletion & PropertyCompletion.HasXOffset) != PropertyCompletion.HasXOffset && (cls.PropertyCompletion & PropertyCompletion.HasXOffset) == PropertyCompletion.HasXOffset)
                        {
                            imageClass.XOffset = cls.XOffset;
                            imageClass.PropertyCompletion = imageClass.PropertyCompletion |
                                                            PropertyCompletion.HasXOffset;
                        }
                        if ((imageClass.PropertyCompletion & PropertyCompletion.HasRepeat) != PropertyCompletion.HasRepeat && (cls.PropertyCompletion & PropertyCompletion.HasRepeat) == PropertyCompletion.HasRepeat)
                        {
                            imageClass.Repeat = cls.Repeat;
                            imageClass.PropertyCompletion = imageClass.PropertyCompletion |
                                                            PropertyCompletion.HasRepeat;
                        }
                        if ((imageClass.PropertyCompletion & PropertyCompletion.HasImage) != PropertyCompletion.HasImage && (cls.PropertyCompletion & PropertyCompletion.HasImage) == PropertyCompletion.HasImage)
                        {
                            imageClass.ImageUrl = cls.ImageUrl;
                            imageClass.PropertyCompletion = imageClass.PropertyCompletion |
                                                            PropertyCompletion.HasImage;
                        }
                        if ((imageClass.PropertyCompletion & PropertyCompletion.HasPaddingBottom) != PropertyCompletion.HasPaddingBottom && (cls.PropertyCompletion & PropertyCompletion.HasPaddingBottom) == PropertyCompletion.HasPaddingBottom)
                        {
                            imageClass.PaddingBottom = cls.PaddingBottom;
                            imageClass.PropertyCompletion = imageClass.PropertyCompletion |
                                                            PropertyCompletion.HasPaddingBottom;
                        }
                        if ((imageClass.PropertyCompletion & PropertyCompletion.HasPaddingTop) != PropertyCompletion.HasPaddingTop && (cls.PropertyCompletion & PropertyCompletion.HasPaddingTop) == PropertyCompletion.HasPaddingTop)
                        {
                            imageClass.PaddingTop = cls.PaddingTop;
                            imageClass.PropertyCompletion = imageClass.PropertyCompletion |
                                                            PropertyCompletion.HasPaddingTop;
                        }
                        if ((imageClass.PropertyCompletion & PropertyCompletion.HasPaddingLeft) != PropertyCompletion.HasPaddingLeft && (cls.PropertyCompletion & PropertyCompletion.HasPaddingLeft) == PropertyCompletion.HasPaddingLeft)
                        {
                            imageClass.PaddingLeft = cls.PaddingLeft;
                            imageClass.PropertyCompletion = imageClass.PropertyCompletion |
                                                            PropertyCompletion.HasPaddingLeft;
                        }
                        if ((imageClass.PropertyCompletion & PropertyCompletion.HasPaddingRight) != PropertyCompletion.HasPaddingRight && (cls.PropertyCompletion & PropertyCompletion.HasPaddingRight) == PropertyCompletion.HasPaddingRight)
                        {
                            imageClass.PaddingRight = cls.PaddingRight;
                            imageClass.PropertyCompletion = imageClass.PropertyCompletion |
                                                            PropertyCompletion.HasPaddingRight;
                        }
                        if ((imageClass.PropertyCompletion & PropertyCompletion.HasWidth) != PropertyCompletion.HasWidth && (cls.PropertyCompletion & PropertyCompletion.HasWidth) == PropertyCompletion.HasWidth)
                        {
                            imageClass.ExplicitWidth = cls.ExplicitWidth;
                            imageClass.PropertyCompletion = imageClass.PropertyCompletion |
                                                            PropertyCompletion.HasWidth;
                        }
                        if ((imageClass.PropertyCompletion & PropertyCompletion.HasHeight) != PropertyCompletion.HasHeight && (cls.PropertyCompletion & PropertyCompletion.HasHeight) == PropertyCompletion.HasHeight)
                        {
                            imageClass.ExplicitHeight = cls.ExplicitHeight;
                            imageClass.PropertyCompletion = imageClass.PropertyCompletion |
                                                            PropertyCompletion.HasHeight;
                        }
                    }
                }

                draftUrls.Add(imageClass);
                if (CanSprite(imageClass))
                    finalUrls.Add(imageClass);
            }
            return finalUrls;
        }

        public string InjectSprite(string originalCss, SpritedImage image)
        {
            return originalCss.Replace(image.CssClass.OriginalClassString, image.Render());
        }

        private bool CanSprite(BackgroundImageClass imageClass)
        {
            if (imageClass.ImageUrl == null)
                return false;

            if (imageClass.Width > 0
                && imageClass.Repeat == RepeatStyle.NoRepeat
                && (imageClass.XOffset.PositionMode == PositionMode.Direction
                    || (imageClass.XOffset.PositionMode != PositionMode.Direction && imageClass.XOffset.Offset <= 0))
                && ((imageClass.YOffset.PositionMode == PositionMode.Direction && (imageClass.YOffset.Direction == Direction.Top || imageClass.Height > 0))
                    || (imageClass.YOffset.PositionMode == PositionMode.Percent && imageClass.YOffset.Offset == 0)
                    || ((imageClass.YOffset.PositionMode == PositionMode.Unit) && (imageClass.YOffset.Offset >= 0 || imageClass.Height > 0))))
                return true;
            return false;
        }

        private bool IsComplete(BackgroundImageClass imageClass)
        {
            return (imageClass.PropertyCompletion &
                   (PropertyCompletion.HasHeight | PropertyCompletion.HasImage | PropertyCompletion.HasPaddingBottom |
                    PropertyCompletion.HasPaddingLeft | PropertyCompletion.HasPaddingRight |
                    PropertyCompletion.HasPaddingTop | PropertyCompletion.HasRepeat | PropertyCompletion.HasWidth |
                    PropertyCompletion.HasXOffset | PropertyCompletion.HasYOffset)) ==
                   (PropertyCompletion.HasHeight | PropertyCompletion.HasImage | PropertyCompletion.HasPaddingBottom |
                    PropertyCompletion.HasPaddingLeft | PropertyCompletion.HasPaddingRight |
                    PropertyCompletion.HasPaddingTop | PropertyCompletion.HasRepeat | PropertyCompletion.HasWidth |
                    PropertyCompletion.HasXOffset | PropertyCompletion.HasYOffset);
        }

        private bool ShouldFlatten(BackgroundImageClass imageClass)
        {
            return (imageClass.PropertyCompletion &
                   (PropertyCompletion.HasImage | PropertyCompletion.HasXOffset | PropertyCompletion.HasYOffset)) ==
                   (PropertyCompletion.HasImage | PropertyCompletion.HasXOffset | PropertyCompletion.HasYOffset);
        }
    }
}