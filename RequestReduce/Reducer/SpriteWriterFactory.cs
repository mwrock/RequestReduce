using RequestReduce.Utilities;

namespace RequestReduce.Reducer
{
    public interface ISpriteWriterFactory
    {
        ISpriteWriter CreateWriter(int surfaceWidth, int surfaceHeight);
    }

    public class SpriteWriterFactory : ISpriteWriterFactory
    {
        private readonly IFileWrapper fileWrapper;

        public SpriteWriterFactory(IFileWrapper fileWrapper)
        {
            this.fileWrapper = fileWrapper;
        }

        public ISpriteWriter CreateWriter(int surfaceWidth, int surfaceHeight)
        {
            return new SpriteWriter(surfaceWidth, surfaceHeight, fileWrapper);
        }
    }
}
