using System.Drawing;
using System.IO;
using RequestReduce.Reducer;
using Xunit;

namespace RequestReduce.Facts.Reducer
{
    public class SpriteWriterFacts
    {
        class TestableSpriteWriter : Testable<SpriteWriter>
        {
            public TestableSpriteWriter()
            {
                
            }
            public static Bitmap Image15X17 = new Bitmap(new MemoryStream(File.ReadAllBytes("testimages\\delete.png")));
            public static Bitmap Image18X18 = new Bitmap(new MemoryStream(File.ReadAllBytes("testimages\\emptyStar.png")));
        }

        public class Ctor
        {
            [Fact]
            public void WillCreateDrawingSurfaceWithDimensionsPassed()
            {
                var testable = new SpriteWriter(10, 20, null);

                Assert.Equal(10, testable.SpriteImage.Width);
                Assert.Equal(20, testable.SpriteImage.Height);
            }
        }

        public class WriteImage
        {
            [Fact]
            public void WillWriteImageToSurfaceAtTheCorrectOffset()
            {
                var testable = new SpriteWriter(33, 18, null);
                testable.WriteImage(TestableSpriteWriter.Image15X17);

                testable.WriteImage(TestableSpriteWriter.Image18X18);

                Assert.Equal(TestableSpriteWriter.Image18X18, testable.SpriteImage.Clone(new Rectangle(15, 0, 18, 18), TestableSpriteWriter.Image18X18.PixelFormat), new BitmapPixelComparer(true));
            }
        }
    }
}
