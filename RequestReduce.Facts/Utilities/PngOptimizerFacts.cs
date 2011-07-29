using System.IO;
using Moq;
using RequestReduce.Configuration;
using RequestReduce.Utilities;
using Xunit;

namespace RequestReduce.Facts.Utilities
{
    public class PngOptimizerFacts
    {
        public PngOptimizerFacts()
        {
            
        }

        [Fact]
        public void WillMakeOriginalPngSmaller()
        {
            var config = new Mock<IRRConfiguration>();
            config.Setup(x => x.SpritePhysicalPath).Returns("testimages");
            var optimizer = new PngOptimizer(new FileWrapper(), config.Object);
            var original = File.ReadAllBytes("testimages\\menu-sprite.png");

            var result = optimizer.OptimizePng(original);

            Assert.True(result.Length < original.Length);
        }

        [Fact]
        public void WillCleanupScratchFile()
        {
            var config = new Mock<IRRConfiguration>();
            config.Setup(x => x.SpritePhysicalPath).Returns("testimages");
            var optimizer = new PngOptimizer(new FileWrapper(), config.Object);
            var original = File.ReadAllBytes("testimages\\menu-sprite.png");
            var fileCount = Directory.GetFiles("testimages").Length;

            optimizer.OptimizePng(original);

            Assert.Equal(fileCount, Directory.GetFiles("testimages").Length);
        }

        [Fact]
        public void WillNotAlterOriginalImage()
        {
            var config = new Mock<IRRConfiguration>();
            config.Setup(x => x.SpritePhysicalPath).Returns("testimages");
            var optimizer = new PngOptimizer(new FileWrapper(), config.Object);
            var original = File.ReadAllBytes("testimages\\menu-sprite.png");

            optimizer.OptimizePng(original);

            Assert.Equal(original.Length, File.ReadAllBytes("testimages\\menu-sprite.png").Length);
        }

    }
}
