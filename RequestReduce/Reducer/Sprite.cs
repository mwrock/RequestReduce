namespace RequestReduce.Reducer
{
    public class Sprite
    {
        public Sprite(int position, int spriteIndex)
        {
            Position = position;
            SpriteIndex = spriteIndex;
        }

        public virtual int Position { get; private set; }
        public virtual string Url { get; set; }
        public virtual int SpriteIndex { get; private set; }
    }
}