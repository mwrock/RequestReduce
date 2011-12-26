using System.Drawing;

namespace RequestReduce.Reducer
{
    public class SpritedImage
    {
        public SpritedImage(int averageColor, BackgroundImageClass cssClass, Bitmap Image)
        {
            AverageColor = averageColor;
            CssClass = cssClass;
            this.Image = Image;
        }

        public Bitmap Image { get; set; }
        public int AverageColor { get; set; }
        public int Position { get; set; }
        public string Url { get; set; }
        public BackgroundImageClass CssClass { get; set; }

        public string Render()
        {
            var yOffset = "0";
            if (CssClass.YOffset.PositionMode == PositionMode.Unit && CssClass.YOffset.Offset > 0)
                yOffset = string.Format("{0}px", CssClass.YOffset.Offset);

            return CssClass.OriginalClassString.Replace("}",
                                        string.Format(";background-image: url('{3}');background-position: -{0}px {1}{2};}}",
                                        Position, yOffset, CssClass.Important ? " !important" : string.Empty, Url));
        }
    }
}