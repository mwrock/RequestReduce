namespace RequestReduce.Reducer
{
    public class Sprite
    {
        public Sprite(int position, string url)
        {
            Position = position;
            Url = url;
        }

        public virtual int Position { get; private set; }
        public virtual string Url { get; private set; }
    }
}