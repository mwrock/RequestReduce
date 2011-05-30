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
using Xunit.Extensions;

namespace RequestReduce.Facts.Reducer
{
    public class SpriteManagerFacts
    {
        class SpriteManagerToTest: SpriteManager
        {
            public SpriteManagerToTest(IWebClientWrapper webClientWrapper, IRRConfiguration config, ISpriteWriterFactory spriteWriterFactory) : base(webClientWrapper, config, spriteWriterFactory)
            {
                MockSpriteContainer = new Mock<ISpriteContainer>();
                MockSpriteContainer.Setup(x => x.GetEnumerator()).Returns(new List<Bitmap>().GetEnumerator());
                base.SpriteContainer = MockSpriteContainer.Object;
                SpritedCssKey = Guid.NewGuid();
            }

            public Mock<ISpriteContainer> MockSpriteContainer { get; set; }
            public new ISpriteContainer SpriteContainer { get { return base.SpriteContainer; } set { base.SpriteContainer = value; } }
        }
        class TestableSpriteManager : Testable<SpriteManagerToTest>
        {
            public TestableSpriteManager()
            {
                Mock<IRRConfiguration>().Setup(x => x.SpriteSizeLimit).Returns(1000);
                Mock<ISpriteWriterFactory>().Setup(x => x.CreateWriter(It.IsAny<int>(), It.IsAny<int>())).Returns(
                    new Mock<ISpriteWriter>().Object);
            }
        }

        public class Add
        {
            [Fact]
            public void WillAddImageToSpriteContainer()
            {
                var testable = new TestableSpriteManager();
                var image = new BackgroundImageClass(""){ImageUrl = ""};

                testable.ClassUnderTest.Add(image);

                testable.ClassUnderTest.MockSpriteContainer.Verify(x => x.AddImage(image), Times.Exactly(1));
            }

            [Fact]
            public void WillIncrementPositionByWidthOfPreviousImage()
            {
                var testable = new TestableSpriteManager();
                testable.ClassUnderTest.MockSpriteContainer.Setup(x => x.Width).Returns(20);

                var result = testable.ClassUnderTest.Add(new BackgroundImageClass("") { ImageUrl = "imageUrl2" });

                Assert.Equal(20, result.Position);
            }

            [Fact]
            public void WillFlushWhenSizePassesThreshold()
            {
                var testable = new TestableSpriteManager();
                testable.Mock<IRRConfiguration>().Setup(x => x.SpriteSizeLimit).Returns(1);
                testable.ClassUnderTest.MockSpriteContainer.Setup(x => x.Size).Returns(1);

                testable.ClassUnderTest.Add(new BackgroundImageClass("") { ImageUrl = "imageUrl" });

                testable.Mock<ISpriteWriterFactory>().Verify(x => x.CreateWriter(It.IsAny<int>(), It.IsAny<int>()), Times.Exactly(1));
            }

            [Fact]
            public void WillReturnPreviousSpriteIfUrlWasSprited()
            {
                var testable = new TestableSpriteManager();
                var sprite = testable.ClassUnderTest.Add(new BackgroundImageClass("") { ImageUrl = "image1" });

                var result = testable.ClassUnderTest.Add(new BackgroundImageClass("") { ImageUrl = "image1" });

                Assert.Equal(sprite, result);
            }

            [Theory,
            InlineData(40, 40, 0, 50, 40, 0),
            InlineData(40, 40, 0, 40, 50, 0),
            InlineData(40, 40, 0, 40, 40, -10)]
            public void WillTreatSameUrlwithDifferentWidthHeightOrXOffsetAsDifferentImagesAndReturnDistinctSprite(int image1Width, int image1Height, int image1XOffset, int image2Width, int image2Height, int image2XOffset)
            {
                var testable = new TestableSpriteManager();
                var sprite = testable.ClassUnderTest.Add(new BackgroundImageClass("") { ImageUrl = "image1", Width = image1Width, Height = image1Height, XOffset = new Position() { Offset = image1XOffset, PositionMode = PositionMode.Unit} });

                var result = testable.ClassUnderTest.Add(new BackgroundImageClass("") { ImageUrl = "image1", Width = image2Width, Height = image2Height, XOffset = new Position() { Offset = image2XOffset, PositionMode = PositionMode.Unit } });

                Assert.NotEqual(sprite, result);
            }

            [Fact]
            public void WillNotAddImageToSpriteContainerIfImageAlreadySprited()
            {
                var testable = new TestableSpriteManager();
                var image = new BackgroundImageClass("") { ImageUrl = "" };
                testable.ClassUnderTest.Add(image);

                testable.ClassUnderTest.Add(image);

                testable.ClassUnderTest.MockSpriteContainer.Verify(x => x.AddImage(image), Times.Exactly(1));
            }

            [Fact]
            public void WillSaveSpriteUrlInCorrectConfigDirectory()
            {
                var testable = new TestableSpriteManager();
                var image = new BackgroundImageClass("") { ImageUrl = "" };
                testable.Mock<IRRConfiguration>().Setup(x => x.SpriteVirtualPath).Returns("spritedir");

                var result = testable.ClassUnderTest.Add(image);

                Assert.True(result.Url.StartsWith("spritedir/"));
            }

            [Fact]
            public void WillSaveSpriteUrlWithKeyInPath()
            {
                var testable = new TestableSpriteManager();
                var image = new BackgroundImageClass("") { ImageUrl = "" };
                testable.Mock<IRRConfiguration>().Setup(x => x.SpriteVirtualPath).Returns("spritedir");

                var result = testable.ClassUnderTest.Add(image);

                Assert.True(result.Url.Contains("/" + testable.ClassUnderTest.SpritedCssKey + "/"));
            }

            [Fact]
            public void WillSaveSpriteUrlWithApngExtension()
            {
                var testable = new TestableSpriteManager();
                var image = new BackgroundImageClass("") { ImageUrl = "" };

                var result = testable.ClassUnderTest.Add(image);

                Assert.True(result.Url.EndsWith(".png"));
            }

            [Fact]
            public void WillThrowInvalidOperationExceptionIfCssKeyIsEmpty()
            {
                var testable = new TestableSpriteManager();
                var image = new BackgroundImageClass("") { ImageUrl = "" };
                testable.ClassUnderTest.SpritedCssKey = Guid.Empty;

                var ex = Assert.Throws<InvalidOperationException>(() => testable.ClassUnderTest.Add(image));

                Assert.NotNull(ex);
            }

            [Fact]
            public void WillSaveSpriteWithAFileNameWithTheSpriteIndex()
            {
                var testable = new TestableSpriteManager();
                testable.ClassUnderTest.MockSpriteContainer.Setup(x => x.Size).Returns(1);
                var mockWriter = new Mock<ISpriteWriter>();
                testable.Mock<ISpriteWriterFactory>().Setup(x => x.CreateWriter(It.IsAny<int>(), It.IsAny<int>())).Returns(mockWriter.Object);
                testable.ClassUnderTest.Flush();
                var image = new BackgroundImageClass("") { ImageUrl = "" };
                testable.ClassUnderTest.MockSpriteContainer.Setup(x => x.GetEnumerator()).Returns(new List<Bitmap>().GetEnumerator());
                testable.ClassUnderTest.SpriteContainer = testable.ClassUnderTest.MockSpriteContainer.Object;

                var result = testable.ClassUnderTest.Add(image);

                Assert.True(result.Url.EndsWith("/sprite2.png"));
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

                testable.Mock<ISpriteWriterFactory>().Verify(x => x.CreateWriter(It.IsAny<int>(), It.IsAny<int>()), Times.Never());
            }

            [Fact]
            public void WillCreateImageWriterWithCorrectDimensions()
            {
                var testable = new TestableSpriteManager();
                testable.ClassUnderTest.MockSpriteContainer.Setup(x => x.Width).Returns(1);
                testable.ClassUnderTest.MockSpriteContainer.Setup(x => x.Height).Returns(1);
                testable.ClassUnderTest.MockSpriteContainer.Setup(x => x.Size).Returns(1);

                testable.ClassUnderTest.Flush();

                testable.Mock<ISpriteWriterFactory>().Verify(x => x.CreateWriter(1, 1), Times.Exactly(1));
            }

            [Fact]
            public void WillWriteEachImage()
            {
                var testable = new TestableSpriteManager();
                var images = new List<Bitmap>() {new Bitmap(1, 1), new Bitmap(2, 2)};
                testable.ClassUnderTest.MockSpriteContainer.Setup(x => x.GetEnumerator()).Returns(images.GetEnumerator());
                var mockWriter = new Mock<ISpriteWriter>();
                testable.Mock<ISpriteWriterFactory>().Setup(x => x.CreateWriter(It.IsAny<int>(), It.IsAny<int>())).Returns(mockWriter.Object);
                testable.ClassUnderTest.MockSpriteContainer.Setup(x => x.Size).Returns(1);

                testable.ClassUnderTest.Flush();

                mockWriter.Verify(x => x.WriteImage(images[0]), Times.Exactly(1));
                mockWriter.Verify(x => x.WriteImage(images[1]), Times.Exactly(1));
            }

            [Fact]
            public void WillSaveWriterToContainerUrlUsingPngMimeType()
            {
                var testable = new TestableSpriteManager();
                var mockWriter = new Mock<ISpriteWriter>();
                testable.Mock<ISpriteWriterFactory>().Setup(x => x.CreateWriter(It.IsAny<int>(), It.IsAny<int>())).Returns(mockWriter.Object);
                testable.ClassUnderTest.MockSpriteContainer.Setup(x => x.Size).Returns(1);

                testable.ClassUnderTest.Flush();

                mockWriter.Verify(x => x.Save(It.IsAny<string>(), "image/png"));
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
                var mockWriter = new Mock<ISpriteWriter>();
                testable.Mock<ISpriteWriterFactory>().Setup(x => x.CreateWriter(It.IsAny<int>(), It.IsAny<int>())).Returns(mockWriter.Object);

                testable.ClassUnderTest.Flush();

                mockWriter.Verify(x => x.Save(It.Is<string>(y => y.StartsWith("spritedir/")), It.IsAny<string>()));
            }

            [Fact]
            public void WillSaveSpriteUrlWithKeyInPath()
            {
                var testable = new TestableSpriteManager();
                testable.Mock<IRRConfiguration>().Setup(x => x.SpriteVirtualPath).Returns("spritedir");
                testable.ClassUnderTest.MockSpriteContainer.Setup(x => x.Size).Returns(1);
                var mockWriter = new Mock<ISpriteWriter>();
                testable.Mock<ISpriteWriterFactory>().Setup(x => x.CreateWriter(It.IsAny<int>(), It.IsAny<int>())).Returns(mockWriter.Object);

                testable.ClassUnderTest.Flush();

                mockWriter.Verify(x => x.Save(It.Is<string>(y => y.Contains("/" + testable.ClassUnderTest.SpritedCssKey + "/")), It.IsAny<string>()));
            }

            [Fact]
            public void WillSaveSpriteUrlWithApngExtension()
            {
                var testable = new TestableSpriteManager();
                testable.ClassUnderTest.MockSpriteContainer.Setup(x => x.Size).Returns(1);
                var mockWriter = new Mock<ISpriteWriter>();
                testable.Mock<ISpriteWriterFactory>().Setup(x => x.CreateWriter(It.IsAny<int>(), It.IsAny<int>())).Returns(mockWriter.Object);

                testable.ClassUnderTest.Flush();

                mockWriter.Verify(x => x.Save(It.Is<string>(y => y.EndsWith(".png")), It.IsAny<string>()));
            }

            [Fact]
            public void WillThrowInvalidOperationExceptionIfCssKeyIsEmpry()
            {
                var testable = new TestableSpriteManager();
                testable.ClassUnderTest.MockSpriteContainer.Setup(x => x.Size).Returns(1);
                var mockWriter = new Mock<ISpriteWriter>();
                testable.Mock<ISpriteWriterFactory>().Setup(x => x.CreateWriter(It.IsAny<int>(), It.IsAny<int>())).Returns(mockWriter.Object);
                testable.ClassUnderTest.SpritedCssKey = Guid.Empty;

                var ex = Assert.Throws<InvalidOperationException>(() => testable.ClassUnderTest.Flush());

                Assert.NotNull(ex);
            }

            [Fact]
            public void WillSaveSpriteWithAFileNameWithTheSpriteIndex()
            {
                var testable = new TestableSpriteManager();
                testable.ClassUnderTest.MockSpriteContainer.Setup(x => x.Size).Returns(1);
                var mockWriter = new Mock<ISpriteWriter>();
                testable.Mock<ISpriteWriterFactory>().Setup(x => x.CreateWriter(It.IsAny<int>(), It.IsAny<int>())).Returns(mockWriter.Object);
                testable.ClassUnderTest.Flush();
                testable.ClassUnderTest.MockSpriteContainer.Setup(x => x.GetEnumerator()).Returns(new List<Bitmap>().GetEnumerator());
                testable.ClassUnderTest.SpriteContainer = testable.ClassUnderTest.MockSpriteContainer.Object;
                testable.ClassUnderTest.MockSpriteContainer.Setup(x => x.Size).Returns(1);
                mockWriter = new Mock<ISpriteWriter>();
                testable.Mock<ISpriteWriterFactory>().Setup(x => x.CreateWriter(It.IsAny<int>(), It.IsAny<int>())).Returns(mockWriter.Object);

                testable.ClassUnderTest.Flush();

                mockWriter.Verify(x => x.Save(It.Is<string>(y => y.EndsWith("/sprite2.png")), It.IsAny<string>()));
            }
        }

        public class Indexer
        {
            [Fact]
            public void WillRetrieveSpriteByOriginalUrl()
            {
                var testable = new TestableSpriteManager();
                var image = new BackgroundImageClass("") { ImageUrl = "" };
                var sprite = testable.ClassUnderTest.Add(image);

                var result = testable.ClassUnderTest[image];

                Assert.Equal(sprite, result);
            }
        }
    }
}
