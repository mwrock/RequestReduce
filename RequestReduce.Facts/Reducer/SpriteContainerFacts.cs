using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using RequestReduce.Reducer;
using RequestReduce.Utilities;
using Xunit;
using Xunit.Extensions;
using RequestReduce.Configuration;

namespace RequestReduce.Facts.Reducer
{
    public class SpriteContainerFacts
    {
        class FakeSpriteContainer : SpriteContainer
        {
            public FakeSpriteContainer(IWebClientWrapper webClientWrapper, IRRConfiguration config) : base(webClientWrapper, config)
            {
            }

            public void AddSpritedImage(SpritedImage image)
            {
                images.Add(image);
            }
        }

        class TestableSpriteContainer : Testable<FakeSpriteContainer>
        {
            public TestableSpriteContainer()
            {
                Mock<IRRConfiguration>().Setup(x => x.IsFullTrust).Returns(true);
            }

            public byte[] Image15X17 = File.ReadAllBytes("testimages\\delete.png");
            public byte[] Image18X18 = File.ReadAllBytes("testimages\\emptyStar.png");

            public static byte[] GetFiveColorImage()
            {
                using(var bitmap = new Bitmap(3, 3, PixelFormat.Format32bppArgb))
                {
                    bitmap.SetPixel(1, 1, Color.Tomato);
                    bitmap.SetPixel(1, 2, Color.Wheat);
                    bitmap.SetPixel(2, 1, Color.Violet);
                    bitmap.SetPixel(2, 2, Color.Teal);
                    using (var stream = new MemoryStream())
                    {
                        bitmap.Save(stream, ImageFormat.Png);
                        return stream.GetBuffer();
                    }
                }
            }

            public static byte[] GetFourColorImage()
            {
                using (var bitmap = new Bitmap(2, 2, PixelFormat.Format32bppArgb))
                {
                    bitmap.SetPixel(1, 1, Color.Turquoise);
                    bitmap.SetPixel(1, 0, Color.DeepSkyBlue);
                    bitmap.SetPixel(0, 1, Color.Violet);
                    using (var stream = new MemoryStream())
                    {
                        bitmap.Save(stream, ImageFormat.Png);
                        return stream.GetBuffer();
                    }
                }
            }

            public static byte[] GetHalfvioletHalfGreyImageImage(Color darkViolet)
            {
                using (var bitmap = new Bitmap(2, 2, PixelFormat.Format32bppArgb))
                {
                    bitmap.SetPixel(1, 1, Color.DarkViolet);
                    bitmap.SetPixel(1, 0, Color.DarkViolet);
                    bitmap.SetPixel(0, 0, Color.DimGray);
                    bitmap.SetPixel(0, 1, Color.DimGray);
                    using (var stream = new MemoryStream())
                    {
                        bitmap.Save(stream, ImageFormat.Png);
                        return stream.GetBuffer();
                    }
                }
            }
        }

        public class AddImage
        {
            [Fact]
            public void SizeWillBeAggregateOfAddedImages()
            {
                var testable = new TestableSpriteContainer();
                var image1 = new BackgroundImageClass("", "http://server/content/style.css") {ImageUrl = "url1"};
                var image2 = new BackgroundImageClass("", "http://server/content/style.css") {ImageUrl = "url2"};
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
                var image1 = new BackgroundImageClass("", "http://server/content/style.css") {ImageUrl = "url1"};
                var image2 = new BackgroundImageClass("", "http://server/content/style.css") {ImageUrl = "url2"};
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
                var image1 = new BackgroundImageClass("", "http://server/content/style.css")
                                 {ImageUrl = "url1", Width = width};
                testable.Mock<IWebClientWrapper>().Setup(Xunit => Xunit.DownloadBytes("url1")).Returns(
                    testable.Image15X17);

                testable.ClassUnderTest.AddImage(image1);

                Assert.Equal(width + 1, testable.ClassUnderTest.Width);
            }

            [Fact]
            public void WillClipLeftEdgeOfBackgroundClassWhenOffsetIsNegative()
            {
                var testable = new TestableSpriteContainer();
                var image1 = new BackgroundImageClass("", "http://server/content/style.css")
                                 {
                                     ImageUrl = "url1",
                                     Width = 5,
                                     XOffset = new Position() {PositionMode = PositionMode.Unit, Offset = -5}
                                 };
                testable.Mock<IWebClientWrapper>().Setup(x => x.DownloadBytes("url1")).Returns(
                    testable.Image15X17);
                var bitMap = new Bitmap(new MemoryStream(testable.Image15X17));

                testable.ClassUnderTest.AddImage(image1);

                Assert.Equal(bitMap.Clone(new Rectangle(5, 0, 5, 17), bitMap.PixelFormat).GraphicsImage(),
                             testable.ClassUnderTest.First().Image, new BitmapPixelComparer(true));
            }

            [Fact]
            public void WillNotClipLeftEdgeOfBackgroundClassWhenOffsetIsPositive()
            {
                var testable = new TestableSpriteContainer();
                var image1 = new BackgroundImageClass("", "http://server/content/style.css")
                                 {
                                     ImageUrl = "url1",
                                     Width = 5,
                                     XOffset = new Position() {PositionMode = PositionMode.Percent, Offset = 50}
                                 };
                testable.Mock<IWebClientWrapper>().Setup(x => x.DownloadBytes("url1")).Returns(
                    testable.Image15X17);
                var bitMap = new Bitmap(new MemoryStream(testable.Image15X17));

                testable.ClassUnderTest.AddImage(image1);

                Assert.Equal(bitMap.Clone(new Rectangle(0, 0, 5, 17), bitMap.PixelFormat).GraphicsImage(),
                             testable.ClassUnderTest.First().Image, new BitmapPixelComparer(true));
            }

            [Fact]
            public void WillClipUpperEdgeOfBackgroundClassWhenOffsetIsNegative()
            {
                var testable = new TestableSpriteContainer();
                var image1 = new BackgroundImageClass("", "http://server/content/style.css")
                                 {
                                     ImageUrl = "url1",
                                     Height = 5,
                                     YOffset = new Position() {PositionMode = PositionMode.Unit, Offset = -5}
                                 };
                testable.Mock<IWebClientWrapper>().Setup(x => x.DownloadBytes("url1")).Returns(
                    testable.Image15X17);
                var bitMap = new Bitmap(new MemoryStream(testable.Image15X17));

                testable.ClassUnderTest.AddImage(image1);

                Assert.Equal(bitMap.Clone(new Rectangle(0, 5, 15, 5), bitMap.PixelFormat).GraphicsImage(),
                             testable.ClassUnderTest.First().Image, new BitmapPixelComparer(true));
            }

            [Fact]
            public void WillNotClipUpperEdgeOfBackgroundClassWhenOffsetIsPositive()
            {
                var testable = new TestableSpriteContainer();
                var image1 = new BackgroundImageClass("", "http://server/content/style.css")
                                 {
                                     ImageUrl = "url1",
                                     Height = 5,
                                     YOffset = new Position() {PositionMode = PositionMode.Percent, Offset = 50}
                                 };
                testable.Mock<IWebClientWrapper>().Setup(x => x.DownloadBytes("url1")).Returns(
                    testable.Image15X17);
                var bitMap = new Bitmap(new MemoryStream(testable.Image15X17));

                testable.ClassUnderTest.AddImage(image1);

                Assert.Equal(bitMap.Clone(new Rectangle(0, 0, 15, 5), bitMap.PixelFormat).GraphicsImage(),
                             testable.ClassUnderTest.First().Image, new BitmapPixelComparer(true));
            }

            [Theory,
             InlineData(10),
             InlineData(20)]
            public void HeightWillBeSizeOfBackgroundClassIfDifferentThanImageHeight(int height)
            {
                var testable = new TestableSpriteContainer();
                var image1 = new BackgroundImageClass("", "http://server/content/style.css")
                                 {ImageUrl = "url1", Height = height};
                testable.Mock<IWebClientWrapper>().Setup(x => x.DownloadBytes("url1")).Returns(
                    testable.Image15X17);

                testable.ClassUnderTest.AddImage(image1);

                Assert.Equal(height, testable.ClassUnderTest.Height);
            }

            [Fact]
            public void HeightWillBeTheTallestOfAddedImages()
            {
                var testable = new TestableSpriteContainer();
                var image1 = new BackgroundImageClass("", "http://server/content/style.css") {ImageUrl = "url1"};
                var image2 = new BackgroundImageClass("", "http://server/content/style.css") {ImageUrl = "url2"};
                testable.Mock<IWebClientWrapper>().Setup(Xunit => Xunit.DownloadBytes("url1")).Returns(
                    testable.Image15X17);
                testable.Mock<IWebClientWrapper>().Setup(Xunit => Xunit.DownloadBytes("url2")).Returns(
                    testable.Image18X18);

                testable.ClassUnderTest.AddImage(image1);
                testable.ClassUnderTest.AddImage(image2);

                Assert.Equal(18, testable.ClassUnderTest.Height);
            }

            [Fact]
            public void WillCountColorsOfAddedImage()
            {
                var testable = new TestableSpriteContainer();
                var fiveColorImage = new BackgroundImageClass("image1",""){ImageUrl = "url"};
                testable.Mock<IWebClientWrapper>().Setup(x => x.DownloadBytes("url")).Returns(TestableSpriteContainer.GetFiveColorImage());

                testable.ClassUnderTest.AddImage(fiveColorImage);

                Assert.Equal(5, testable.ClassUnderTest.Colors);
            }

            [Fact]
            public void ColorCountWillBe0InRestrictedTrust()
            {
                var testable = new TestableSpriteContainer();
                var fiveColorImage = new BackgroundImageClass("image1", "") { ImageUrl = "url" };
                testable.Mock<IWebClientWrapper>().Setup(x => x.DownloadBytes("url")).Returns(TestableSpriteContainer.GetFiveColorImage());
                testable.Mock<IRRConfiguration>().Setup(x => x.IsFullTrust).Returns(false);

                testable.ClassUnderTest.AddImage(fiveColorImage);

                Assert.Equal(0, testable.ClassUnderTest.Colors);
            }

            [Fact]
            public void WillCountUniqueColorsOfAddedImages()
            {
                var testable = new TestableSpriteContainer();
                var fiveColorImage = new BackgroundImageClass("image1", "") { ImageUrl = "url" };
                testable.Mock<IWebClientWrapper>().Setup(x => x.DownloadBytes("url")).Returns(TestableSpriteContainer.GetFiveColorImage());
                var fourColorImage = new BackgroundImageClass("image2", "") { ImageUrl = "url2" };
                testable.Mock<IWebClientWrapper>().Setup(x => x.DownloadBytes("url2")).Returns(TestableSpriteContainer.GetFourColorImage());

                testable.ClassUnderTest.AddImage(fiveColorImage);
                testable.ClassUnderTest.AddImage(fourColorImage);

                Assert.Equal(7, testable.ClassUnderTest.Colors);
            }

            [Fact]
            public void UniqueColorsOfAddedImagesWillBe0WhenNotInFullTrust()
            {
                var testable = new TestableSpriteContainer();
                var fiveColorImage = new BackgroundImageClass("image1", "") { ImageUrl = "url" };
                testable.Mock<IWebClientWrapper>().Setup(x => x.DownloadBytes("url")).Returns(TestableSpriteContainer.GetFiveColorImage());
                var fourColorImage = new BackgroundImageClass("image2", "") { ImageUrl = "url2" };
                testable.Mock<IWebClientWrapper>().Setup(x => x.DownloadBytes("url2")).Returns(TestableSpriteContainer.GetFourColorImage());
                testable.Mock<IRRConfiguration>().Setup(x => x.IsFullTrust).Returns(false);

                testable.ClassUnderTest.AddImage(fiveColorImage);
                testable.ClassUnderTest.AddImage(fourColorImage);

                Assert.Equal(0, testable.ClassUnderTest.Colors);
            }

            [Fact]
            public void WillCalculateAverageColorsOfAddedImages()
            {
                var testable = new TestableSpriteContainer();
                var halfvioletHalfGreyImage = new BackgroundImageClass("image1", "") { ImageUrl = "url" };
                testable.Mock<IWebClientWrapper>().Setup(x => x.DownloadBytes("url")).Returns(TestableSpriteContainer.GetHalfvioletHalfGreyImageImage(Color.DarkViolet));
                var color1 = Color.DarkViolet.ToArgb();
                var color2 = Color.DimGray.ToArgb();

                var result = testable.ClassUnderTest.AddImage(halfvioletHalfGreyImage);

                Assert.Equal((color1+color2)/2, result.AverageColor);
            }

            [Fact]
            public void AverageColorsOfAddedImagesWillBe0WhenNotInFullTrust()
            {
                var testable = new TestableSpriteContainer();
                var halfvioletHalfGreyImage = new BackgroundImageClass("image1", "") { ImageUrl = "url" };
                testable.Mock<IWebClientWrapper>().Setup(x => x.DownloadBytes("url")).Returns(TestableSpriteContainer.GetHalfvioletHalfGreyImageImage(Color.DarkViolet));
                var color1 = Color.DarkViolet.ToArgb();
                var color2 = Color.DimGray.ToArgb();
                testable.Mock<IRRConfiguration>().Setup(x => x.IsFullTrust).Returns(false);

                var result = testable.ClassUnderTest.AddImage(halfvioletHalfGreyImage);

                Assert.Equal(0, result.AverageColor);
            }

        }

        public class Enumerator
        {
            [Fact]
            public void WillReturnAllImages()
            {
                var testable = new TestableSpriteContainer();
                var image1 = new BackgroundImageClass("", "http://server/content/style.css") { ImageUrl = "url1" };
                var image2 = new BackgroundImageClass("", "http://server/content/style.css") { ImageUrl = "url2" };
                testable.Mock<IWebClientWrapper>().Setup(x => x.DownloadBytes("url1")).Returns(
                    testable.Image15X17);
                testable.Mock<IWebClientWrapper>().Setup(x => x.DownloadBytes("url2")).Returns(
                    testable.Image18X18);

                testable.ClassUnderTest.AddImage(image1);
                testable.ClassUnderTest.AddImage(image2);

                Assert.Contains(image1, testable.ClassUnderTest.Select(x => x.CssClass));
                Assert.Contains(image2, testable.ClassUnderTest.Select(x => x.CssClass));
            }

            [Fact]
            public void WillReturnAllImagesOrderedByColor()
            {
                var testable = new TestableSpriteContainer();
                var image1 = new SpritedImage(20, null, null);
                var image2 = new SpritedImage(10, null, null);
                testable.ClassUnderTest.AddSpritedImage(image1);
                testable.ClassUnderTest.AddSpritedImage(image2);

                var results = testable.ClassUnderTest.ToArray();

                Assert.Equal(image1, results[1]);
                Assert.Equal(image2, results[0]);
            }
        }
    }
}
