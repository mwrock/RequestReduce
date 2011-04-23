using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using Moq;
using RequestReduce.Configuration;
using RequestReduce.Reducer;
using RequestReduce.Utilities;
using Xunit;

namespace RequestReduce.Facts.Reducer
{
    public class SpriteManagerFacts
    {
        class SpriteManagerToTest: SpriteManager
        {
            public SpriteManagerToTest(IWebClientWrapper webClientWrapper, IConfigurationWrapper configWrapper, IFileWrapper fileWrapper, HttpContextBase httpContext, ISpriteWriterFactory spriteWriterFactory) : base(webClientWrapper, configWrapper, fileWrapper, httpContext, spriteWriterFactory)
            {
            }

            public new SpriteContainer SpriteContainer { get { return base.SpriteContainer; } }
        }
        class TestableSpriteManager : Testable<SpriteManagerToTest>
        {
            public TestableSpriteManager()
            {
                int size = 1;
                Mock<IWebClientWrapper>().Setup(x => x.DownloadImage(It.IsAny<string>(), out size)).Returns(new Bitmap(1,1));
                Mock<HttpContextBase>().Setup(x => x.Server.MapPath(It.IsAny<string>())).Returns((string s) => s);
                Mock<IFileWrapper>().Setup(x => x.OpenStream(It.IsAny<string>())).Returns(new MemoryStream());
                Mock<IConfigurationWrapper>().Setup(x => x.SpriteSizeLimit).Returns(1000);
            }
        }

        public class Add
        {
            [Fact]
            public void WillReturnSpriteUrlInCorrectConfigDirectory()
            {
                var testable = new TestableSpriteManager();
                testable.Mock<IConfigurationWrapper>().Setup(x => x.SpriteDirectory).Returns("spritedir");

                var result = testable.ClassUnderTest.Add("imageUrl");

                Assert.True(result.Url.StartsWith("spritedir/"));
            }

            [Fact]
            public void WillReturnSpriteUrlWithAGuidName()
            {
                var testable = new TestableSpriteManager();
                testable.Mock<IConfigurationWrapper>().Setup(x => x.SpriteDirectory).Returns("spritedir");
                Guid guid;

                var result = testable.ClassUnderTest.Add("imageUrl");

                Assert.True(Guid.TryParse(result.Url.Substring("spritedir/".Length, result.Url.Length - "spritedir/".Length - ".css".Length), out guid));
            }

            [Fact]
            public void WillReturnSpriteUrlWithApngExtension()
            {
                var testable = new TestableSpriteManager();

                var result = testable.ClassUnderTest.Add("imageUrl");

                Assert.True(result.Url.EndsWith(".png"));
            }

            [Fact]
            public void WillAddImageToUnflushedImages()
            {
                var testable = new TestableSpriteManager();
                var imageBitmap = new Bitmap(1,1);
                int size;
                testable.Mock<IWebClientWrapper>().Setup(x => x.DownloadImage("imageUrl", out size)).Returns(imageBitmap);

                var result = testable.ClassUnderTest.Add("imageUrl");

                Assert.True(testable.ClassUnderTest.SpriteContainer.Contains(imageBitmap));
            }

            [Fact]
            public void WillIncrementPositionByWidthOfPreviousImage()
            {
                var testable = new TestableSpriteManager();
                var imageBitmap = new Bitmap(20, 1);
                int size;
                testable.Mock<IWebClientWrapper>().Setup(x => x.DownloadImage(It.IsAny<string>(), out size)).Returns(imageBitmap);
                testable.ClassUnderTest.Add("imageUrl");

                var result = testable.ClassUnderTest.Add("imageUrl2");

                Assert.Equal(20, result.Position);
            }

            [Fact]
            public void WillFlushToFileWhenSizePassesThreshold()
            {
                var testable = new TestableSpriteManager();
                testable.Mock<IConfigurationWrapper>().Setup(x => x.SpriteSizeLimit).Returns(1);

                var result = testable.ClassUnderTest.Add("imageUrl");

                testable.Mock<IFileWrapper>().Verify(x => x.OpenStream(result.Url), Times.Exactly(1));
            }

            [Fact]
            public void WillSpriteContainerAreaHaveAccurateWidthAndHeight()
            {
                var testable = new TestableSpriteManager();

                var result = testable.ClassUnderTest.Add("imageUrl");

                testable.Mock<IFileWrapper>().Verify(x => x.OpenStream(result.Url), Times.Exactly(1));
            }
        }
    }
}
