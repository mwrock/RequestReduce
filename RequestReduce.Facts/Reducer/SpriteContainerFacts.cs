using System.Drawing;
using System.IO;
using System.Linq;
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

        public class AddImage
        {
            [Fact]
            public void SizeWillBeAggregateOfAddedImages()
            {
                var testable = new TestableSpriteContainer();
                var image1 = new BackgroundImageClass("", "http://server/content/style.css"){ImageUrl = "url1"};
                var image2 = new BackgroundImageClass("", "http://server/content/style.css") { ImageUrl = "url2" };
                testable.Mock<IWebClientWrapper>().Setup(Xunit => Xunit.DownloadBytes("url1")).Returns(
                    testable.Image15X17);
                testable.Mock<IWebClientWrapper>().Setup(Xunit => Xunit.DownloadBytes("url2")).Returns(
                    testable.Image18X18);

                testable.ClassUnderTest.AddImage(image1);
                testable.ClassUnderTest.AddImage(image2);

                Assert.Equal(testable.Image15X17.Length + testable.Image18X18.Length, testable.ClassUnderTest.Size);
            }

            [Fact]
            public void WidthWillBeAggregateOfAddedImageWidthsPlusOnePixelEach()
            {
                var testable = new TestableSpriteContainer();
                var image1 = new BackgroundImageClass("", "http://server/content/style.css") { ImageUrl = "url1" };
                var image2 = new BackgroundImageClass("", "http://server/content/style.css") { ImageUrl = "url2" };
                testable.Mock<IWebClientWrapper>().Setup(Xunit => Xunit.DownloadBytes("url1")).Returns(
                    testable.Image15X17);
                testable.Mock<IWebClientWrapper>().Setup(Xunit => Xunit.DownloadBytes("url2")).Returns(
                    testable.Image18X18);

                testable.ClassUnderTest.AddImage(image1);
                testable.ClassUnderTest.AddImage(image2);

                Assert.Equal(35, testable.ClassUnderTest.Width);
            }

            [Theory,
            InlineData(10),
            InlineData(20)]
            public void WidthWillBeSizeOfBackgroundClassPluOneIfDifferentThanImageWidth(int width)
            {
                var testable = new TestableSpriteContainer();
                var image1 = new BackgroundImageClass("", "http://server/content/style.css") { ImageUrl = "url1", Width = width};
                testable.Mock<IWebClientWrapper>().Setup(Xunit => Xunit.DownloadBytes("url1")).Returns(
                    testable.Image15X17);

                testable.ClassUnderTest.AddImage(image1);

                Assert.Equal(width+1, testable.ClassUnderTest.Width);
            }

            [Fact]
            public void WillClipLeftEdgeOfBackgroundClassWhenOffsetIsNegative()
            {
                var testable = new TestableSpriteContainer();
                var image1 = new BackgroundImageClass("", "http://server/content/style.css") { ImageUrl = "url1", Width = 5, XOffset = new Position(){ PositionMode = PositionMode.Unit, Offset = -5}};
                testable.Mock<IWebClientWrapper>().Setup(x => x.DownloadBytes("url1")).Returns(
                    testable.Image15X17);
                var bitMap = new Bitmap(new MemoryStream(testable.Image15X17));

                testable.ClassUnderTest.AddImage(image1);

                Assert.Equal(bitMap.Clone(new Rectangle(5, 0, 5, 17), bitMap.PixelFormat).GraphicsImage(), testable.ClassUnderTest.First(), new  BitmapPixelComparer(true));
            }

            [Fact]
            public void WillNotClipLeftEdgeOfBackgroundClassWhenOffsetIsPositive()
            {
                var testable = new TestableSpriteContainer();
                var image1 = new BackgroundImageClass("", "http://server/content/style.css") { ImageUrl = "url1", Width = 5, XOffset = new Position() { PositionMode = PositionMode.Percent, Offset = 50 } };
                testable.Mock<IWebClientWrapper>().Setup(x => x.DownloadBytes("url1")).Returns(
                    testable.Image15X17);
                var bitMap = new Bitmap(new MemoryStream(testable.Image15X17));

                testable.ClassUnderTest.AddImage(image1);

                Assert.Equal(bitMap.Clone(new Rectangle(0, 0, 5, 17), bitMap.PixelFormat).GraphicsImage(), testable.ClassUnderTest.First(), new BitmapPixelComparer(true));
            }

            [Fact]
            public void WillClipUpperEdgeOfBackgroundClassWhenOffsetIsNegative()
            {
                var testable = new TestableSpriteContainer();
                var image1 = new BackgroundImageClass("", "http://server/content/style.css") { ImageUrl = "url1", Height = 5, YOffset = new Position() { PositionMode = PositionMode.Unit, Offset = -5 } };
                testable.Mock<IWebClientWrapper>().Setup(x => x.DownloadBytes("url1")).Returns(
                    testable.Image15X17);
                var bitMap = new Bitmap(new MemoryStream(testable.Image15X17));

                testable.ClassUnderTest.AddImage(image1);

                Assert.Equal(bitMap.Clone(new Rectangle(0, 5, 15, 5), bitMap.PixelFormat).GraphicsImage(), testable.ClassUnderTest.First(), new BitmapPixelComparer(true));
            }

            [Fact]
            public void WillNotClipUpperEdgeOfBackgroundClassWhenOffsetIsPositive()
            {
                var testable = new TestableSpriteContainer();
                var image1 = new BackgroundImageClass("", "http://server/content/style.css") { ImageUrl = "url1", Height = 5, YOffset = new Position() { PositionMode = PositionMode.Percent, Offset = 50 } };
                testable.Mock<IWebClientWrapper>().Setup(x => x.DownloadBytes("url1")).Returns(
                    testable.Image15X17);
                var bitMap = new Bitmap(new MemoryStream(testable.Image15X17));

                testable.ClassUnderTest.AddImage(image1);

                Assert.Equal(bitMap.Clone(new Rectangle(0, 0, 15, 5), bitMap.PixelFormat).GraphicsImage(), testable.ClassUnderTest.First(), new BitmapPixelComparer(true));
            }

            [Theory,
            InlineData(10),
            InlineData(20)]
            public void HeightWillBeSizeOfBackgroundClassIfDifferentThanImageHeight(int height)
            {
                var testable = new TestableSpriteContainer();
                var image1 = new BackgroundImageClass("", "http://server/content/style.css") { ImageUrl = "url1", Height = height };
                testable.Mock<IWebClientWrapper>().Setup(x => x.DownloadBytes("url1")).Returns(
                    testable.Image15X17);

                testable.ClassUnderTest.AddImage(image1);

                Assert.Equal(height, testable.ClassUnderTest.Height);
            }

            [Fact]
            public void HeightWillBeTheTallestOfAddedImages()
            {
                var testable = new TestableSpriteContainer();
                var image1 = new BackgroundImageClass("", "http://server/content/style.css") { ImageUrl = "url1" };
                var image2 = new BackgroundImageClass("", "http://server/content/style.css") { ImageUrl = "url2" };
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
                var image1 = new BackgroundImageClass("", "http://server/content/style.css") { ImageUrl = "url1" };
                var image2 = new BackgroundImageClass("", "http://server/content/style.css") { ImageUrl = "url2" };
                testable.Mock<IWebClientWrapper>().Setup(x => x.DownloadBytes("url1")).Returns(
                    testable.Image15X17);
                testable.Mock<IWebClientWrapper>().Setup(x => x.DownloadBytes("url2")).Returns(
                    testable.Image18X18);

                testable.ClassUnderTest.AddImage(image1);
                testable.ClassUnderTest.AddImage(image2);

                Assert.Contains(bitmap1.GraphicsImage(), testable.ClassUnderTest, new BitmapPixelComparer(true));
                Assert.Contains(bitmap2.GraphicsImage(), testable.ClassUnderTest, new BitmapPixelComparer());
            }
        }
    }
}
