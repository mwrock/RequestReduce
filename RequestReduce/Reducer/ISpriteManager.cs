namespace RequestReduce.Reducer
{
    public interface ISpriteManager
    {
        Sprite this[string imageUrl] { get; }
        Sprite Add(string imageUrl);
        void Flush();
    }
}