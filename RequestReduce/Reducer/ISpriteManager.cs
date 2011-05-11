namespace RequestReduce.Reducer
{
    public interface ISpriteManager
    {
        Sprite this[BackgroungImageClass image] { get; }
        Sprite Add(BackgroungImageClass imageUrl);
        void Flush();
    }
}