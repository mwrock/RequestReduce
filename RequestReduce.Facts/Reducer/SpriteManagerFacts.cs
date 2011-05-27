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
                MockSpriteContainer.Setup(x => x.Url).Returns(SpriteContainer.Url);
                MockSpriteContainer.Setup(x => x.FilePath).Returns(SpriteContainer.FilePath);
                MockSpriteContainer.Setup(x => x.GetEnumerator()).Returns(new List<Bitmap>().GetEnumerator());
                base.SpriteContainer = MockSpriteContainer.Object;
            }

            public Mock<ISpriteContainer> MockSpriteContainer { get; private set; }
            public new ISpriteContainer SpriteContainer { get { return base.SpriteContainer; } }
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
                testable.ClassUnderTest.MockSpriteContainer.Setup(x => x.FilePath).Returns("myurl");
                var mockWriter = new Mock<ISpriteWriter>();
                testable.Mock<ISpriteWriterFactory>().Setup(x => x.CreateWriter(It.IsAny<int>(), It.IsAny<int>())).Returns(mockWriter.Object);
                testable.ClassUnderTest.MockSpriteContainer.Setup(x => x.Size).Returns(1);

                testable.ClassUnderTest.Flush();

                mockWriter.Verify(x => x.Save("myurl", "image/png"));
            }

            [Fact]
            public void WillResetSpriteContainerAfterFlush()
            {
                var testable = new TestableSpriteManager();
                testable.ClassUnderTest.MockSpriteContainer.Setup(x => x.Width).Returns(20);

                testable.ClassUnderTest.Flush();

                Assert.Equal(0, testable.ClassUnderTest.SpriteContainer.Width);
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
