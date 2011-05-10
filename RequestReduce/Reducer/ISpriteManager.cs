namespace RequestReduce.Reducer
{
    public interface ISpriteManager
    {
        Sprite this[string imageUrl] { get; }
        Sprite Add(BackgroungImageClass imageUrl);
        void Flush();
    }
}