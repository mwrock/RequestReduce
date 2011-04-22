using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
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
            public SpriteManagerToTest(IWebClientWrapper webClientWrapper, IConfigurationWrapper configWrapper) : base(webClientWrapper, configWrapper)
            {
            }

            public IList<Bitmap> UnflushedImages { get { return unflushedImages; } }
        }
        class TestableSpriteManager : Testable<SpriteManagerToTest>
        {
            public TestableSpriteManager()
            {
                Mock<IWebClientWrapper>().Setup(x => x.DownloadImage(It.IsAny<string>())).Returns(new Bitmap(1,1));
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
                testable.Mock<IWebClientWrapper>().Setup(x => x.DownloadImage("imageUrl")).Returns(imageBitmap);

                var result = testable.ClassUnderTest.Add("imageUrl");

                Assert.True(testable.ClassUnderTest.UnflushedImages.Contains(imageBitmap));
            }

            [Fact]
            public void WillIncrementPositionByWidthOfPreviousImage()
            {
                var testable = new TestableSpriteManager();
                var imageBitmap = new Bitmap(20, 1);
                testable.Mock<IWebClientWrapper>().Setup(x => x.DownloadImage(It.IsAny<string>())).Returns(imageBitmap);
                testable.ClassUnderTest.Add("imageUrl");

                var result = testable.ClassUnderTest.Add("imageUrl2");

                Assert.Equal(20, result.Position);
            }
        }
    }
}
