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
            public SpriteManagerToTest(IWebClientWrapper webClientWrapper, IRRConfiguration config, IUriBuilder uriBuilder, IStore store, IPngOptimizer pngOptimizer) : base(webClientWrapper, config, uriBuilder, store, pngOptimizer)
            {
                MockSpriteContainer = new Mock<ISpriteContainer>();
                MockSpriteContainer.Setup(x => x.GetEnumerator()).Returns(new List<OrderedImage>().GetEnumerator());
                MockSpriteContainer.Setup(x => x.Width).Returns(1);
                MockSpriteContainer.Setup(x => x.Height).Returns(1);
                base.SpriteContainer = MockSpriteContainer.Object;
                SpritedCssKey = Guid.NewGuid();
            }

            public Mock<ISpriteContainer> MockSpriteContainer { get; set; }
            public new ISpriteContainer SpriteContainer { get { return base.SpriteContainer; } set { base.SpriteContainer = value; } }
            public int SpriteIndex { get { return spriteIndex; } }
            public void AddSpriteToList(Sprite sprite)
            {
                spriteList.Add(new ImageMetadata(){Url = Guid.NewGuid().ToString()}, sprite);
            }
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

                testable.ClassUnderTest.MockSpriteContainer.Verify(x => x.AddImage(image, It.Is<Sprite>(s => s.SpriteIndex == 1)), Times.Exactly(1));
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

                testable.ClassUnderTest.MockSpriteContainer.Verify(x => x.AddImage(image, It.IsAny<Sprite>()), Times.Exactly(1));
            }

            [Fact]
            public void WillIncrementedSpriteIndexAfterFlush()
            {
                var testable = new TestableSpriteManager();
                testable.ClassUnderTest.MockSpriteContainer.Setup(x => x.Size).Returns(1);
                testable.ClassUnderTest.Flush();
                var image = new BackgroundImageClass("", "http://server/content/style.css") { ImageUrl = "" };
                testable.ClassUnderTest.MockSpriteContainer.Setup(x => x.GetEnumerator()).Returns(new List<OrderedImage>().GetEnumerator());
                testable.ClassUnderTest.SpriteContainer = testable.ClassUnderTest.MockSpriteContainer.Object;

                var result = testable.ClassUnderTest.Add(image);

                Assert.Equal(2, result.SpriteIndex);
            }

            [Fact]
            public void WillNotIncrementIndexofFileInvokingFlush()
            {
                var testable = new TestableSpriteManager();
                testable.Mock<IRRConfiguration>().Setup(x => x.SpriteSizeLimit).Returns(1);
                testable.ClassUnderTest.MockSpriteContainer.Setup(x => x.Size).Returns(1);
                var image = new BackgroundImageClass("", "http://server/content/style.css") { ImageUrl = "" };
                testable.ClassUnderTest.MockSpriteContainer.Setup(x => x.GetEnumerator()).Returns(new List<OrderedImage>().GetEnumerator());
                testable.ClassUnderTest.SpriteContainer = testable.ClassUnderTest.MockSpriteContainer.Object;

                var result = testable.ClassUnderTest.Add(image);

                Assert.Equal(1, result.SpriteIndex);
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
                testable.Mock<IPngOptimizer>().Setup(x => x.OptimizePng(It.IsAny<byte[]>(), It.IsAny<int>(), false)).Callback
                    <byte[], int, bool>((a, b, c) => bytes = a).Returns(() => bytes);

                testable.ClassUnderTest.Flush();

                var bitMap = new Bitmap(new MemoryStream(bytes));
                Assert.Equal(1, bitMap.Width);
                Assert.Equal(1, bitMap.Height);
            }

            [Fact]
            public void WillWriteEachImage()
            {
                var testable = new TestableSpriteManager();
                testable.ClassUnderTest.MockSpriteContainer.Setup(x => x.Width).Returns(35);
                testable.ClassUnderTest.MockSpriteContainer.Setup(x => x.Height).Returns(18);
                var images = new List<OrderedImage>()
                                 {
                                     new OrderedImage() {Image = TestableSpriteManager.Image15X17, Sprite = new Sprite(1)},
                                     new OrderedImage() {Image = TestableSpriteManager.Image18X18, Sprite = new Sprite(1)}
                                 };
                testable.ClassUnderTest.MockSpriteContainer.Setup(x => x.GetEnumerator()).Returns(images.GetEnumerator());
                testable.ClassUnderTest.MockSpriteContainer.Setup(x => x.Size).Returns(1);
                byte[] bytes = null;
                testable.Mock<IPngOptimizer>().Setup(x => x.OptimizePng(It.IsAny<byte[]>(), It.IsAny<int>(), false)).Callback
                    <byte[], int, bool>((a, b, c) => bytes = a).Returns(() => bytes); ;

                testable.ClassUnderTest.Flush();

                var bitMap = new Bitmap(new MemoryStream(bytes));
                Assert.Equal(TestableSpriteManager.Image15X17.GraphicsImage(), bitMap.Clone(new Rectangle(0, 0, 15, 17), TestableSpriteManager.Image15X17.PixelFormat), new BitmapPixelComparer(true));
                Assert.Equal(TestableSpriteManager.Image18X18.GraphicsImage(), bitMap.Clone(new Rectangle(16, 0, 18, 18), TestableSpriteManager.Image18X18.PixelFormat), new BitmapPixelComparer(true));
            }

            [Fact]
            public void WillIncrementPositionByWidthOfPreviousImage()
            {
                var testable = new TestableSpriteManager();
                testable.ClassUnderTest.MockSpriteContainer.Setup(x => x.Width).Returns(35);
                testable.ClassUnderTest.MockSpriteContainer.Setup(x => x.Height).Returns(18);
                var testableSprite = new Sprite(1);
                var images = new List<OrderedImage>()
                                 {
                                     new OrderedImage() {Image = TestableSpriteManager.Image15X17, Sprite = new Sprite(1)},
                                     new OrderedImage() {Image = TestableSpriteManager.Image18X18, Sprite = testableSprite}
                                 };
                testable.ClassUnderTest.MockSpriteContainer.Setup(x => x.GetEnumerator()).Returns(images.GetEnumerator());
                testable.ClassUnderTest.MockSpriteContainer.Setup(x => x.Size).Returns(1);
                byte[] bytes = null;
                testable.Mock<IPngOptimizer>().Setup(x => x.OptimizePng(It.IsAny<byte[]>(), It.IsAny<int>(), false)).Callback
                    <byte[], int, bool>((a, b, c) => bytes = a).Returns(() => bytes);

                testable.ClassUnderTest.Flush();

                Assert.Equal(16, testableSprite.Position);
            }

            [Fact]
            public void WillSaveWriterToContainerUrlUsingPngMimeType()
            {
                var testable = new TestableSpriteManager();
                byte[] bytes = null;
                testable.Mock<IPngOptimizer>().Setup(x => x.OptimizePng(It.IsAny<byte[]>(), It.IsAny<int>(), false)).Callback
                    <byte[], int, bool>((a, b, c) => bytes = a).Returns(() => bytes);
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

                Assert.True(url.Contains("/" + testable.ClassUnderTest.SpritedCssKey.RemoveDashes() + "-"));
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
            public void WillSaveSpriteWithAFileNameWithThebyteHashInTheFileName()
            {
                var testable = new TestableSpriteManager();
                testable.ClassUnderTest.MockSpriteContainer.Setup(x => x.Size).Returns(1);
                testable.ClassUnderTest.Flush();
                testable.ClassUnderTest.MockSpriteContainer.Setup(x => x.GetEnumerator()).Returns(new List<OrderedImage>().GetEnumerator());
                testable.ClassUnderTest.SpriteContainer = testable.ClassUnderTest.MockSpriteContainer.Object;
                testable.ClassUnderTest.MockSpriteContainer.Setup(x => x.Size).Returns(1);
                var url = string.Empty;
                byte[] bytes = new byte[]{1,2,3};
                testable.Mock<IPngOptimizer>().Setup(x => x.OptimizePng(It.IsAny<byte[]>(), It.IsAny<int>(), false)).Returns(bytes);
                testable.Mock<IStore>().Setup(x => x.Save(bytes, It.IsAny<string>(), null)).Callback
                    <byte[], string, string>((a, b, c) =>
                    {
                        url = b;
                    });

                testable.ClassUnderTest.Flush();

                Assert.True(url.EndsWith(string.Format("{0}.png", Hasher.Hash(bytes).RemoveDashes())));
            }

            [Fact]
            public void WillAddUrlsToSpritesUponFlush()
            {
                var testable = new TestableSpriteManager();
                testable.ClassUnderTest.MockSpriteContainer.Setup(x => x.Size).Returns(1);
                var sprite1 = new Sprite(1);
                var sprite2 = new Sprite(1);
                testable.ClassUnderTest.AddSpriteToList(sprite1);
                testable.ClassUnderTest.AddSpriteToList(sprite2);
                var url = string.Empty;
                testable.Mock<IStore>().Setup(x => x.Save(It.IsAny<byte[]>(), It.IsAny<string>(), null)).Callback
                    <byte[], string, string>((a, b, c) => url = b);

                testable.ClassUnderTest.Flush();

                Assert.Equal(url, sprite1.Url);
                Assert.Equal(url, sprite2.Url);
            }

            [Fact]
            public void WillOptimizeImageIfOptimizationIsEnabled()
            {
                var testable = new TestableSpriteManager();
                byte[] bytes = null;
                testable.Mock<IRRConfiguration>().Setup(x => x.ImageOptimizationDisabled).Returns(false);
                testable.Mock<IRRConfiguration>().Setup(x => x.ImageOptimizationCompressionLevel).Returns(2);
                testable.Mock<IStore>().Setup(x => x.Save(It.IsAny<byte[]>(), It.IsAny<string>(), null)).Callback
                    <byte[], string, string>((a, b, c) => bytes = a);
                testable.ClassUnderTest.MockSpriteContainer.Setup(x => x.Size).Returns(1);
                var optimizedBytes = new byte[] {5, 5, 5, 5, 5};
                testable.Mock<IPngOptimizer>().Setup(x => x.OptimizePng(It.IsAny<byte[]>(), 2, false)).Returns(optimizedBytes);

                testable.ClassUnderTest.Flush();

                Assert.Equal(optimizedBytes, bytes);
            }

            [Fact]
            public void WillNotOptimizeImageIfOptimizationIsDisabled()
            {
                var testable = new TestableSpriteManager();
                testable.Mock<IRRConfiguration>().Setup(x => x.ImageOptimizationDisabled).Returns(true);
                testable.ClassUnderTest.MockSpriteContainer.Setup(x => x.Size).Returns(1);
                var optimizedBytes = new byte[0];
                testable.Mock<IPngOptimizer>().Setup(x => x.OptimizePng(It.IsAny<byte[]>(), It.IsAny<int>(), false)).
                    Callback<byte[], int, bool>((a, b, c) => optimizedBytes = a).Returns(() => optimizedBytes);

                testable.ClassUnderTest.Flush();

                Assert.Empty(optimizedBytes);
            }

            [Theory,
            InlineData(2),
            InlineData(3)]
            public void WillPassConfiguredCompressionLevelToOptimizer(int expectedCompression)
            {
                var testable = new TestableSpriteManager();
                int compression = 0;
                byte[] bytes = null;
                testable.Mock<IRRConfiguration>().Setup(x => x.ImageOptimizationDisabled).Returns(false);
                testable.Mock<IRRConfiguration>().Setup(x => x.ImageOptimizationCompressionLevel).Returns(expectedCompression);
                testable.ClassUnderTest.MockSpriteContainer.Setup(x => x.Size).Returns(1);
                testable.Mock<IPngOptimizer>().Setup(x => x.OptimizePng(It.IsAny<byte[]>(), It.IsAny<int>(), false)).
                    Callback<byte[], int, bool>((a, b, c) => { compression = b;
                                                                 bytes = a;
                    }).Returns(() => bytes);

                testable.ClassUnderTest.Flush();

                Assert.Equal(expectedCompression, compression);
            }

            [Theory,
            InlineData(true),
            InlineData(false)]
            public void WillPassConfiguredQuantiaztionEnablementToOptimizer(bool expectedToBeDisabled)
            {
                var testable = new TestableSpriteManager();
                bool isQuantizationDisabled = false;
                byte[] bytes = null;
                testable.Mock<IRRConfiguration>().Setup(x => x.ImageOptimizationDisabled).Returns(false);
                testable.Mock<IRRConfiguration>().Setup(x => x.ImageQuantizationDisabled).Returns(expectedToBeDisabled);
                testable.ClassUnderTest.MockSpriteContainer.Setup(x => x.Size).Returns(1);
                testable.Mock<IPngOptimizer>().Setup(
                    x => x.OptimizePng(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<bool>())).Callback
                    <byte[], int, bool>((a, b, c) =>
                                            {
                                                isQuantizationDisabled=c;
                                                bytes=a;
                                            }).Returns(() => bytes);

                testable.ClassUnderTest.Flush();

                Assert.Equal(expectedToBeDisabled, isQuantizationDisabled);
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
