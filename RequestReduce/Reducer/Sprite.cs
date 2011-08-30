namespace RequestReduce.Reducer
{
    public class Sprite
    {
        public Sprite(int spriteIndex)
        {
            SpriteIndex = spriteIndex;
        }

        public virtual int Position { get; set; }
        public virtual string Url { get; set; }
        public virtual int SpriteIndex { get; private set; }
    }
}