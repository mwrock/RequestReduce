namespace RequestReduce.Reducer
{
    public interface ISpriteManager
    {
        Sprite this[BackgroundImageClass image] { get; }
        Sprite Add(BackgroundImageClass imageUrl);
        void Flush();
    }
}