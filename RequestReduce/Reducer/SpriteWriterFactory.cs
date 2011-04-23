namespace RequestReduce.Reducer
{
    public interface ISpriteWriterFactory
    {
        ISpriteWriter CreateWriter(int surfaceWidth, int surfaceHeight);
    }

    public class SpriteWriterFactory : ISpriteWriterFactory
    {
        public ISpriteWriter CreateWriter(int surfaceWidth, int surfaceHeight)
        {
            return new SpriteWriter(surfaceWidth, surfaceHeight);
        }
    }
}
