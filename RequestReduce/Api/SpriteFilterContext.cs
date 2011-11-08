using RequestReduce.Reducer;

namespace RequestReduce.Api
{
    public class SpriteFilterContext : IFilterContext
    {
        public SpriteFilterContext(BackgroundImageClass image)
        {
            BackgroundImage = image;
        }
        public BackgroundImageClass BackgroundImage { private set; get; }
    }
}