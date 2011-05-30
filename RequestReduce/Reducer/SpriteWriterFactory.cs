using RequestReduce.Store;
using RequestReduce.Utilities;

namespace RequestReduce.Reducer
{
    public interface ISpriteWriterFactory
    {
        ISpriteWriter CreateWriter(int surfaceWidth, int surfaceHeight);
    }

    public class SpriteWriterFactory : ISpriteWriterFactory
    {
        private readonly IStore store;

        public SpriteWriterFactory(IStore store)
        {
            this.store = store;
        }

        public ISpriteWriter CreateWriter(int surfaceWidth, int surfaceHeight)
        {
            return new SpriteWriter(surfaceWidth, surfaceHeight, store);
        }
    }
}
