using System;
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
    }
}
