using System.Drawing;

namespace RequestReduce.Reducer
{
    public class SpritedImage
    {
        public SpritedImage(int averageColor, BackgroundImageClass cssClass, Bitmap image)
        {
            AverageColor = averageColor;
            CssClass = cssClass;
            Image = image;
        }

        public Bitmap Image { get; set; }
        public int AverageColor { get; set; }
        public int Position { get; set; }
        public string Url { get; set; }
        public BackgroundImageClass CssClass { get; set; }

        public string Render()
        {
            var newClass = CssClass.OriginalClassString.ToLower().Replace(CssClass.ImageUrl.ToLower(), Url); 
            var yOffset = "0";
            if (CssClass.YOffset.PositionMode == PositionMode.Unit && CssClass.YOffset.Offset > 0)
                yOffset = string.Format("{0}px", CssClass.YOffset.Offset);
            if(newClass.IndexOf(Url, System.StringComparison.Ordinal) == -1)
                return newClass.Replace("}", string.Format(";background-image: url('{3}');background-position: -{0}px {1}{2};}}",
                                        Position, yOffset, CssClass.Important ? " !important" : string.Empty, Url)); 

            return newClass.Replace("}",
                                        string.Format(";background-position: -{0}px {1}{2};}}",
                                        Position, yOffset, CssClass.Important ? " !important" : string.Empty));
        }
    }
}