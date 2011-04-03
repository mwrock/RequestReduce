using System;

namespace RequestReduce.Reducer
{
    public class SpriteManager : ISpriteManager
    {
        public virtual bool Contains(string imageUrl)
        {
            throw new NotImplementedException();
        }

        public virtual Sprite this[string imageUrl]
        {
            get { throw new NotImplementedException(); }
        }

        public virtual Sprite Add(string imageUrl)
        {
            throw new NotImplementedException();
        }

        public virtual void Flush()
        {
            throw new NotImplementedException();
        }
    }
}