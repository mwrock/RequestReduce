using System;
using System.Drawing;
using System.IO;
using System.Linq;
using RequestReduce.Configuration;
using RequestReduce.Reducer;
using Xunit;

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

                testable.ClassUnderTest.AddImage(testable.Image15X17);
                testable.ClassUnderTest.AddImage(testable.Image18X18);

                Assert.Equal(testable.Image15X17.Length + testable.Image18X18.Length, testable.ClassUnderTest.Size);
            }

            [Fact]
            public void WidthWillBeAggregateOfAddedImageWidths()
            {
                var testable = new TestableSpriteContainer();

                testable.ClassUnderTest.AddImage(testable.Image15X17);
                testable.ClassUnderTest.AddImage(testable.Image18X18);

                Assert.Equal(33, testable.ClassUnderTest.Width);
            }

            [Fact]
            public void HeightWillBeTheTallestOfAddedImages()
            {
                var testable = new TestableSpriteContainer();

                testable.ClassUnderTest.AddImage(testable.Image15X17);
                testable.ClassUnderTest.AddImage(testable.Image18X18);

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

                testable.ClassUnderTest.AddImage(testable.Image15X17);
                testable.ClassUnderTest.AddImage(testable.Image18X18);

                Assert.Contains(bitmap1, testable.ClassUnderTest, new BitmapPixelComparer());
                Assert.Contains(bitmap2, testable.ClassUnderTest, new BitmapPixelComparer());
            }
        }
    }
}
