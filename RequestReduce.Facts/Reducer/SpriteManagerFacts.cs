using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Moq;
using RequestReduce.Configuration;
using RequestReduce.Reducer;
using RequestReduce.Store;
using RequestReduce.Utilities;
using Xunit;
using Xunit.Extensions;
using UriBuilder = RequestReduce.Utilities.UriBuilder;

namespace RequestReduce.Facts.Reducer
{
    public class SpriteManagerFacts
    {
        class SpriteManagerToTest: SpriteManager
        {
            public SpriteManagerToTest(IWebClientWrapper webClientWrapper, IRRConfiguration config, IUriBuilder uriBuilder, IStore store) : base(webClientWrapper, config, uriBuilder, store)
            {
                MockSpriteContainer = new Mock<ISpriteContainer>();
                MockSpriteContainer.Setup(x => x.GetEnumerator()).Returns(new List<Bitmap>().GetEnumerator());
                MockSpriteContainer.Setup(x => x.Width).Returns(1);
                MockSpriteContainer.Setup(x => x.Height).Returns(1);
                base.SpriteContainer = MockSpriteContainer.Object;
                SpritedCssKey = Guid.NewGuid();
            }

            public Mock<ISpriteContainer> MockSpriteContainer { get; set; }
            public new ISpriteContainer SpriteContainer { get { return base.SpriteContainer; } set { base.SpriteContainer = value; } }
            public int SpriteIndex { get { return spriteIndex; } }
        }
        class TestableSpriteManager : Testable<SpriteManagerToTest>
        {
            public TestableSpriteManager()
            {
                Mock<IRRConfiguration>().Setup(x => x.SpriteSizeLimit).Returns(1000);
                Inject<IUriBuilder>(new UriBuilder(Mock<IRRConfiguration>().Object));
            }

            public static Bitmap Image15X17 = CreateFileImage("testimages\\delete.png");
            public static Bitmap Image18X18 = CreateFileImage("testimages\\emptyStar.png");

            private static Bitmap CreateFileImage(string path)
            {
                return new Bitmap(new MemoryStream(File.ReadAllBytes(path)));
            }
        }

        public class Add
        {
            [Fact]
            public void WillAddImageToSpriteContainer()
            {
                var testable = new TestableSpriteManager();
                var image = new BackgroundImageClass("", "http://server/content/style.css"){ImageUrl = ""};

                testable.ClassUnderTest.Add(image);

                testable.ClassUnderTest.MockSpriteContainer.Verify(x => x.AddImage(image), Times.Exactly(1));
            }

            [Fact]
            public void WillIncrementPositionByWidthOfPreviousImage()
            {
                var testable = new TestableSpriteManager();
                testable.ClassUnderTest.MockSpriteContainer.Setup(x => x.Width).Returns(20);

                var result = testable.ClassUnderTest.Add(new BackgroundImageClass("", "http://server/content/style.css") { ImageUrl = "imageUrl2" });

                Assert.Equal(20, result.Position);
            }

            [Fact]
            public void WillFlushWhenSizePassesThreshold()
            {
                var testable = new TestableSpriteManager();
                testable.Mock<IRRConfiguration>().Setup(x => x.SpriteSizeLimit).Returns(1);
                testable.ClassUnderTest.MockSpriteContainer.Setup(x => x.Size).Returns(1);

                testable.ClassUnderTest.Add(new BackgroundImageClass("", "http://server/content/style.css") { ImageUrl = "imageUrl" });

                Assert.Equal(2, testable.ClassUnderTest.SpriteIndex);
            }

            [Fact]
            public void WillReturnPreviousSpriteIfUrlWasSprited()
            {
                var testable = new TestableSpriteManager();
                var sprite = testable.ClassUnderTest.Add(new BackgroundImageClass("", "http://server/content/style.css") { ImageUrl = "image1" });

                var result = testable.ClassUnderTest.Add(new BackgroundImageClass("", "http://server/content/style.css") { ImageUrl = "image1" });

                Assert.Equal(sprite, result);
            }

            [Theory,
            InlineData(40, 40, 0, 50, 40, 0),
            InlineData(40, 40, 0, 40, 50, 0),
            InlineData(40, 40, 0, 40, 40, -10)]
            public void WillTreatSameUrlwithDifferentWidthHeightOrXOffsetAsDifferentImagesAndReturnDistinctSprite(int image1Width, int image1Height, int image1XOffset, int image2Width, int image2Height, int image2XOffset)
            {
                var testable = new TestableSpriteManager();
                var sprite = testable.ClassUnderTest.Add(new BackgroundImageClass("", "http://server/content/style.css") { ImageUrl = "image1", Width = image1Width, Height = image1Height, XOffset = new Position() { Offset = image1XOffset, PositionMode = PositionMode.Unit} });

                var result = testable.ClassUnderTest.Add(new BackgroundImageClass("", "http://server/content/style.css") { ImageUrl = "image1", Width = image2Width, Height = image2Height, XOffset = new Position() { Offset = image2XOffset, PositionMode = PositionMode.Unit } });

                Assert.NotEqual(sprite, result);
            }

            [Fact]
            public void WillNotAddImageToSpriteContainerIfImageAlreadySprited()
            {
                var testable = new TestableSpriteManager();
                var image = new BackgroundImageClass("", "http://server/content/style.css") { ImageUrl = "" };
                testable.ClassUnderTest.Add(image);

                testable.ClassUnderTest.Add(image);

                testable.ClassUnderTest.MockSpriteContainer.Verify(x => x.AddImage(image), Times.Exactly(1));
            }

            [Fact]
            public void WillSaveSpriteUrlInCorrectConfigDirectory()
            {
                var testable = new TestableSpriteManager();
                var image = new BackgroundImageClass("", "http://server/content/style.css") { ImageUrl = "" };
                testable.Mock<IRRConfiguration>().Setup(x => x.SpriteVirtualPath).Returns("spritedir");

                var result = testable.ClassUnderTest.Add(image);

                Assert.True(result.Url.StartsWith("spritedir/"));
            }

            [Fact]
            public void WillSaveSpriteUrlWithKeyInPath()
            {
                var testable = new TestableSpriteManager();
                var image = new BackgroundImageClass("", "http://server/content/style.css") { ImageUrl = "" };
                testable.Mock<IRRConfiguration>().Setup(x => x.SpriteVirtualPath).Returns("spritedir");

                var result = testable.ClassUnderTest.Add(image);

                Assert.True(result.Url.Contains("/" + testable.ClassUnderTest.SpritedCssKey + "-"));
            }

            [Fact]
            public void WillSaveSpriteUrlWithApngExtension()
            {
                var testable = new TestableSpriteManager();
                var image = new BackgroundImageClass("", "http://server/content/style.css") { ImageUrl = "" };

                var result = testable.ClassUnderTest.Add(image);

                Assert.True(result.Url.EndsWith(".png"));
            }

            [Fact]
            public void WillThrowInvalidOperationExceptionIfCssKeyIsEmpty()
            {
                var testable = new TestableSpriteManager();
                var image = new BackgroundImageClass("", "http://server/content/style.css") { ImageUrl = "" };
                testable.ClassUnderTest.SpritedCssKey = Guid.Empty;

                var ex = Assert.Throws<InvalidOperationException>(() => testable.ClassUnderTest.Add(image));

                Assert.NotNull(ex);
            }

            [Fact]
            public void WillSaveSpriteWithAFileNameWithTheSpriteIndex()
            {
                var testable = new TestableSpriteManager();
                testable.ClassUnderTest.MockSpriteContainer.Setup(x => x.Size).Returns(1);
                testable.ClassUnderTest.Flush();
                var image = new BackgroundImageClass("", "http://server/content/style.css") { ImageUrl = "" };
                testable.ClassUnderTest.MockSpriteContainer.Setup(x => x.GetEnumerator()).Returns(new List<Bitmap>().GetEnumerator());
                testable.ClassUnderTest.SpriteContainer = testable.ClassUnderTest.MockSpriteContainer.Object;

                var result = testable.ClassUnderTest.Add(image);

                Assert.True(result.Url.EndsWith("-sprite2.png"));
            }
        }

        public class Flush
        {
            [Fact]
            public void WillNotCreateImageWriterIfContainerIsEmpty()
            {
                var testable = new TestableSpriteManager();
                testable.ClassUnderTest.MockSpriteContainer.Setup(x => x.Size).Returns(0);

                testable.ClassUnderTest.Flush();

                Assert.Equal(1, testable.ClassUnderTest.SpriteIndex);
            }

            [Fact]
            public void WillCreateImageWriterWithCorrectDimensions()
            {
                var testable = new TestableSpriteManager();
                testable.ClassUnderTest.MockSpriteContainer.Setup(x => x.Width).Returns(1);
                testable.ClassUnderTest.MockSpriteContainer.Setup(x => x.Height).Returns(1);
                testable.ClassUnderTest.MockSpriteContainer.Setup(x => x.Size).Returns(1);
                byte[] bytes = null;
                testable.Mock<IStore>().Setup(x => x.Save(It.IsAny<byte[]>(), It.IsAny<string>(), null)).Callback
                    <byte[],string,string>((a,b,c) => bytes = a);

                testable.ClassUnderTest.Flush();

                var bitMap = new Bitmap(new MemoryStream(bytes));
                Assert.Equal(1, bitMap.Width);
                Assert.Equal(1, bitMap.Height);
            }

            [Fact]
            public void WillWriteEachImage()
            {
                var testable = new TestableSpriteManager();
                testable.ClassUnderTest.MockSpriteContainer.Setup(x => x.Width).Returns(33);
                testable.ClassUnderTest.MockSpriteContainer.Setup(x => x.Height).Returns(18);
                var images = new List<Bitmap>() { TestableSpriteManager.Image15X17, TestableSpriteManager.Image18X18 };
                testable.ClassUnderTest.MockSpriteContainer.Setup(x => x.GetEnumerator()).Returns(images.GetEnumerator());
                testable.ClassUnderTest.MockSpriteContainer.Setup(x => x.Size).Returns(1);
                byte[] bytes = null;
                testable.Mock<IStore>().Setup(x => x.Save(It.IsAny<byte[]>(), It.IsAny<string>(), null)).Callback
                    <byte[], string, string>((a, b, c) => bytes = a);

                testable.ClassUnderTest.Flush();

                var bitMap = new Bitmap(new MemoryStream(bytes));
                Assert.Equal(TestableSpriteManager.Image15X17.GraphicsImage(), bitMap.Clone(new Rectangle(0, 0, 15, 17), TestableSpriteManager.Image15X17.PixelFormat), new BitmapPixelComparer(true));
                Assert.Equal(TestableSpriteManager.Image18X18.GraphicsImage(), bitMap.Clone(new Rectangle(15, 0, 18, 18), TestableSpriteManager.Image18X18.PixelFormat), new BitmapPixelComparer(true));
            }

            [Fact]
            public void WillSaveWriterToContainerUrlUsingPngMimeType()
            {
                var testable = new TestableSpriteManager();
                byte[] bytes = null;
                testable.Mock<IStore>().Setup(x => x.Save(It.IsAny<byte[]>(), It.IsAny<string>(), null)).Callback
                    <byte[], string, string>((a, b, c) => bytes = a);
                testable.ClassUnderTest.MockSpriteContainer.Setup(x => x.Size).Returns(1);

                testable.ClassUnderTest.Flush();

                var bitMap = new Bitmap(new MemoryStream(bytes));
                Assert.Equal(ImageFormat.Png, bitMap.RawFormat);
            }

            [Fact]
            public void WillResetSpriteContainerAfterFlush()
            {
                var testable = new TestableSpriteManager();
                testable.ClassUnderTest.MockSpriteContainer.Setup(x => x.Width).Returns(20);

                testable.ClassUnderTest.Flush();

                Assert.Equal(0, testable.ClassUnderTest.SpriteContainer.Width);
            }

            [Fact]
            public void WillSaveSpriteUrlInCorrectConfigDirectory()
            {
                var testable = new TestableSpriteManager();
                testable.Mock<IRRConfiguration>().Setup(x => x.SpriteVirtualPath).Returns("spritedir");
                testable.ClassUnderTest.MockSpriteContainer.Setup(x => x.Size).Returns(1);
                var url = string.Empty;
                testable.Mock<IStore>().Setup(x => x.Save(It.IsAny<byte[]>(), It.IsAny<string>(), null)).Callback
                    <byte[], string, string>((a, b, c) => url = b);

                testable.ClassUnderTest.Flush();

                Assert.True(url.StartsWith("spritedir/"));
            }

            [Fact]
            public void WillSaveSpriteUrlWithKeyInPath()
            {
                var testable = new TestableSpriteManager();
                testable.Mock<IRRConfiguration>().Setup(x => x.SpriteVirtualPath).Returns("spritedir");
                testable.ClassUnderTest.MockSpriteContainer.Setup(x => x.Size).Returns(1);
                var url = string.Empty;
                testable.Mock<IStore>().Setup(x => x.Save(It.IsAny<byte[]>(), It.IsAny<string>(), null)).Callback
                    <byte[], string, string>((a, b, c) => url = b);

                testable.ClassUnderTest.Flush();

                Assert.True(url.Contains("/" + testable.ClassUnderTest.SpritedCssKey + "-"));
            }

            [Fact]
            public void WillSaveSpriteUrlWithApngExtension()
            {
                var testable = new TestableSpriteManager();
                testable.ClassUnderTest.MockSpriteContainer.Setup(x => x.Size).Returns(1);
                var url = string.Empty;
                testable.Mock<IStore>().Setup(x => x.Save(It.IsAny<byte[]>(), It.IsAny<string>(), null)).Callback
                    <byte[], string, string>((a, b, c) => url = b);

                testable.ClassUnderTest.Flush();

                Assert.True(url.EndsWith(".png"));
            }

            [Fact]
            public void WillThrowInvalidOperationExceptionIfCssKeyIsEmpty()
            {
                var testable = new TestableSpriteManager();
                testable.ClassUnderTest.MockSpriteContainer.Setup(x => x.Size).Returns(1);
                testable.ClassUnderTest.SpritedCssKey = Guid.Empty;

                var ex = Assert.Throws<InvalidOperationException>(() => testable.ClassUnderTest.Flush());

                Assert.NotNull(ex);
            }

            [Fact]
            public void WillSaveSpriteWithAFileNameWithTheSpriteIndex()
            {
                var testable = new TestableSpriteManager();
                testable.ClassUnderTest.MockSpriteContainer.Setup(x => x.Size).Returns(1);
                testable.ClassUnderTest.Flush();
                testable.ClassUnderTest.MockSpriteContainer.Setup(x => x.GetEnumerator()).Returns(new List<Bitmap>().GetEnumerator());
                testable.ClassUnderTest.SpriteContainer = testable.ClassUnderTest.MockSpriteContainer.Object;
                testable.ClassUnderTest.MockSpriteContainer.Setup(x => x.Size).Returns(1);
                var url = string.Empty;
                testable.Mock<IStore>().Setup(x => x.Save(It.IsAny<byte[]>(), It.IsAny<string>(), null)).Callback
                    <byte[], string, string>((a, b, c) => url = b);

                testable.ClassUnderTest.Flush();

                Assert.True(url.EndsWith("-sprite2.png"));
            }
        }

        public class Indexer
        {
            [Fact]
            public void WillRetrieveSpriteByOriginalUrl()
            {
                var testable = new TestableSpriteManager();
                var image = new BackgroundImageClass("", "http://server/content/style.css") { ImageUrl = "" };
                var sprite = testable.ClassUnderTest.Add(image);

                var result = testable.ClassUnderTest[image];

                Assert.Equal(sprite, result);
            }
        }
    }
}
