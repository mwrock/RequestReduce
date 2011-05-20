using System;
using System.Drawing;
using System.IO;
using System.Linq;
using RequestReduce.Configuration;
using RequestReduce.Reducer;
using RequestReduce.Utilities;
using Xunit;
using Xunit.Extensions;

namespace RequestReduce.Facts.Reducer
{
    public class SpriteContainerFacts
    {
        class TestableSpriteContainer : Testable<SpriteContainer>
        {
            public TestableSpriteContainer()
            {
                
            }

            public byte[] Image15X17 = File.ReadAllBytes("testimages\\delete.png");
            public byte[] Image18X18 = File.ReadAllBytes("testimages\\emptyStar.png");
        }

        public class Ctor
        {
            [Fact]
            public void WillReturnSpriteUrlInCorrectConfigDirectory()
            {
                var testable = new TestableSpriteContainer();
                testable.Mock<IConfigurationWrapper>().Setup(x => x.SpriteDirectory).Returns("spritedir");

                var result = testable.ClassUnderTest;

                Assert.True(result.Url.StartsWith("spritedir/"));
            }

            [Fact]
            public void WillReturnSpriteUrlWithAGuidName()
            {
                var testable = new TestableSpriteContainer();
                testable.Mock<IConfigurationWrapper>().Setup(x => x.SpriteDirectory).Returns("spritedir");
                Guid guid;

                var result = testable.ClassUnderTest;

                Assert.True(Guid.TryParse(result.Url.Substring("spritedir/".Length, result.Url.Length - "spritedir/".Length - ".css".Length), out guid));
            }

            [Fact]
            public void WillReturnSpriteUrlWithApngExtension()
            {
                var testable = new TestableSpriteContainer();

                var result = testable.ClassUnderTest;

                Assert.True(result.Url.EndsWith(".png"));
            }
        }

        public class AddImage
        {
            [Fact]
            public void SizeWillBeAggregateOfAddedImages()
            {
                var testable = new TestableSpriteContainer();
                var image1 = new BackgroungImageClass(""){ImageUrl = "url1"};
                var image2 = new BackgroungImageClass("") { ImageUrl = "url2" };
                testable.Mock<IWebClientWrapper>().Setup(Xunit => Xunit.DownloadBytes("url1")).Returns(
                    testable.Image15X17);
                testable.Mock<IWebClientWrapper>().Setup(Xunit => Xunit.DownloadBytes("url2")).Returns(
                    testable.Image18X18);

                testable.ClassUnderTest.AddImage(image1);
                testable.ClassUnderTest.AddImage(image2);

                Assert.Equal(testable.Image15X17.Length + testable.Image18X18.Length, testable.ClassUnderTest.Size);
            }

            [Fact]
            public void WidthWillBeAggregateOfAddedImageWidths()
            {
                var testable = new TestableSpriteContainer();
                var image1 = new BackgroungImageClass("") { ImageUrl = "url1" };
                var image2 = new BackgroungImageClass("") { ImageUrl = "url2" };
                testable.Mock<IWebClientWrapper>().Setup(Xunit => Xunit.DownloadBytes("url1")).Returns(
                    testable.Image15X17);
                testable.Mock<IWebClientWrapper>().Setup(Xunit => Xunit.DownloadBytes("url2")).Returns(
                    testable.Image18X18);

                testable.ClassUnderTest.AddImage(image1);
                testable.ClassUnderTest.AddImage(image2);

                Assert.Equal(33, testable.ClassUnderTest.Width);
            }

            [Theory,
            InlineData(10),
            InlineData(20)]
            public void WidthWillBeSizeOfBackgroundClassIfDifferentThanImageWidth(int width)
            {
                var testable = new TestableSpriteContainer();
                var image1 = new BackgroungImageClass("") { ImageUrl = "url1", Width = width};
                testable.Mock<IWebClientWrapper>().Setup(Xunit => Xunit.DownloadBytes("url1")).Returns(
                    testable.Image15X17);

                testable.ClassUnderTest.AddImage(image1);

                Assert.Equal(width, testable.ClassUnderTest.Width);
            }

            [Theory,
            InlineData(10),
            InlineData(20)]
            public void HeightWillBeSizeOfBackgroundClassIfDifferentThanImageHeight(int height)
            {
                var testable = new TestableSpriteContainer();
                var image1 = new BackgroungImageClass("") { ImageUrl = "url1", Height = height };
                testable.Mock<IWebClientWrapper>().Setup(Xunit => Xunit.DownloadBytes("url1")).Returns(
                    testable.Image15X17);

                testable.ClassUnderTest.AddImage(image1);

                Assert.Equal(height, testable.ClassUnderTest.Height);
            }

            [Fact]
            public void HeightWillBeTheTallestOfAddedImages()
            {
                var testable = new TestableSpriteContainer();
                var image1 = new BackgroungImageClass("") { ImageUrl = "url1" };
                var image2 = new BackgroungImageClass("") { ImageUrl = "url2" };
                testable.Mock<IWebClientWrapper>().Setup(Xunit => Xunit.DownloadBytes("url1")).Returns(
                    testable.Image15X17);
                testable.Mock<IWebClientWrapper>().Setup(Xunit => Xunit.DownloadBytes("url2")).Returns(
                    testable.Image18X18);

                testable.ClassUnderTest.AddImage(image1);
                testable.ClassUnderTest.AddImage(image2);

                Assert.Equal(18, testable.ClassUnderTest.Height);
            }
        }

        public class Enumerator
        {
            [Fact]
            public void WillReturnAllImages()
            {
                var testable = new TestableSpriteContainer();
                var bitmap1 = new Bitmap(new MemoryStream(testable.Image15X17));
                var bitmap2 = new Bitmap(new MemoryStream(testable.Image18X18));
                var image1 = new BackgroungImageClass("") { ImageUrl = "url1" };
                var image2 = new BackgroungImageClass("") { ImageUrl = "url2" };
                testable.Mock<IWebClientWrapper>().Setup(Xunit => Xunit.DownloadBytes("url1")).Returns(
                    testable.Image15X17);
                testable.Mock<IWebClientWrapper>().Setup(Xunit => Xunit.DownloadBytes("url2")).Returns(
                    testable.Image18X18);

                testable.ClassUnderTest.AddImage(image1);
                testable.ClassUnderTest.AddImage(image2);

                Assert.Contains(bitmap1.GraphicsImage(), testable.ClassUnderTest, new BitmapPixelComparer(true));
                Assert.Contains(bitmap2.GraphicsImage(), testable.ClassUnderTest, new BitmapPixelComparer());
            }
        }
    }
}
