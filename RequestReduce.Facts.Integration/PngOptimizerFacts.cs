using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Moq;
using RequestReduce.Configuration;
using RequestReduce.Reducer;
using RequestReduce.Utilities;
using nQuant;
using Xunit;

namespace RequestReduce.Facts.Integration
{
    public class PngOptimizerFacts
    {
        [Fact]
        public void CompressionWillMakeOriginalPngSmaller()
        {
            var config = new Mock<IRRConfiguration>();
            config.Setup(x => x.SpritePhysicalPath).Returns("testimages");
            var optimizer = new PngOptimizer(new FileWrapper(), config.Object, new WuQuantizer());
            var original = File.ReadAllBytes("testimages\\menu-sprite.png");

            var result = optimizer.OptimizePng(original, 5, true);

            Assert.True(result.Length < original.Length);
        }

        [Fact]
        public void QuantizationWillMakeOriginalPngSmaller()
        {
            var config = new Mock<IRRConfiguration>();
            var optimizer = new PngOptimizer(new FileWrapper(), config.Object, new WuQuantizer());
            var original = File.ReadAllBytes("testimages\\menu-sprite.png");

            var result = optimizer.OptimizePng(original, 0, false);

            Assert.True(result.Length < original.Length);
        }

        [Fact]
        public void WillCleanupScratchFile()
        {
            var config = new Mock<IRRConfiguration>();
            config.Setup(x => x.SpritePhysicalPath).Returns("testimages");
            var optimizer = new PngOptimizer(new FileWrapper(), config.Object, new WuQuantizer());
            var original = File.ReadAllBytes("testimages\\menu-sprite.png");
            var fileCount = Directory.GetFiles("testimages").Length;

            optimizer.OptimizePng(original, 5, true);

            Assert.Equal(fileCount, Directory.GetFiles("testimages").Length);
        }

        [Fact]
        public void WillNotAlterOriginalImage()
        {
            var config = new Mock<IRRConfiguration>();
            config.Setup(x => x.SpritePhysicalPath).Returns("testimages");
            var optimizer = new PngOptimizer(new FileWrapper(), config.Object, new WuQuantizer());
            var original = File.ReadAllBytes("testimages\\menu-sprite.png");

            optimizer.OptimizePng(original, 5, false);

            Assert.Equal(original.Length, File.ReadAllBytes("testimages\\menu-sprite.png").Length);
        }

        [Fact]
        [Trait("type", "manual_adhoc")]
        public void QuantTest()
        {
            var opt = new WuQuantizer();
            var source = new Bitmap("c:\\requestreduce\\requestreduce.sampleweb\\vstest\\vstest.png");

            var image = opt.QuantizeImage(source, 10, 70);
            image.Dispose();

            opt = new WuQuantizer();
            var watch = new Stopwatch();
            watch.Start();
            image = opt.QuantizeImage(source, 10, 70);
            var ms = watch.ElapsedMilliseconds;

            image.Save("c:\\requestreduce\\requestreduce.sampleweb\\vstest\\vstest-quant2.png", ImageFormat.Png);

            image.Dispose();
            source.Dispose();
        }

    }
}
