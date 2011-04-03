namespace RequestReduce.Reducer
{
    public class Sprite
    {
        public Sprite(int position, string url)
        {
            Position = position;
            Url = url;
        }

        public int Position { get; private set; }
        public string Url { get; private set; }
    }
}