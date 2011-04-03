namespace RequestReduce.Reducer
{
    public interface ISpriteManager
    {
        bool Contains(string imageUrl);
        Sprite this[string imageUrl] { get; }
        Sprite Add(string imageUrl);
        void Flush();
    }
}