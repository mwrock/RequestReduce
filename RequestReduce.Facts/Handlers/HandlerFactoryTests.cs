using System;
using System.Web;
using RequestReduce.Configuration;
using RequestReduce.Handlers;
using RequestReduce.IOC;
using RequestReduce.Utilities;
using StructureMap;
using StructureMap.AutoMocking;
using Xunit;
using Xunit.Extensions;

namespace RequestReduce.Facts.Handlers
{
    class HandlerFactoryTests
    {
        class TestableHandlerFactory : Testable<RequestReduce.Handlers.HandlerFactory>
        {
            public TestableHandlerFactory()
            {
                Mock<IRRConfiguration>().Setup(x => x.ResourceAbsolutePath).Returns("/RRContent");
            }
        }

        public class ResolveHandler
        {
            class FakeHandler : IHttpHandler
            {
                public void ProcessRequest(HttpContext context)
                {
                }

                public bool IsReusable
                {
                    get { return false; }
                }
            }

            [Fact]
            public void WillResolveAddedMap()
            {
                var testable = new TestableHandlerFactory();
                testable.ClassUnderTest.AddHandlerMap((x,y) => x.AbsolutePath.EndsWith(".fake") ? new FakeHandler() : null);

                var result = testable.ClassUnderTest.ResolveHandler(new Uri("http://host.com/page.fake"), false);

                Assert.IsType<FakeHandler>(result);
            }

            [Fact]
            public void WillResolveToNullIfThereAreNoMatchingMaps()
            {
                var testable = new TestableHandlerFactory();
                testable.ClassUnderTest.AddHandlerMap((x, y) => x.AbsolutePath.EndsWith(".fake") ? new FakeHandler() : null);

                var result = testable.ClassUnderTest.ResolveHandler(new Uri("http://host.com/page.notfake"), false);

                Assert.Null(result);
            }

            [Fact]
            public void WillResolveToNullIfThereAreNoMapsGiven()
            {
                var testable = new TestableHandlerFactory();

                var result = testable.ClassUnderTest.ResolveHandler(new Uri("http://host.com/page.fake"), false);

                Assert.Null(result);
            }

            [Theory]
            [InlineData("http://host/RRContent/dashboard", true, typeof(DashboardHandler))]
            [InlineData("http://host/RRContent/dashboard", false, null)]
            [InlineData("http://host/content/someresource.less", false, null)]
            [InlineData("http://host/RRContent/child/someresource", false, null)]
            [InlineData("http://host/RRContents/someresource", false, null)]
            [InlineData("http://host/RRContent/flush/9879879879879987", true, typeof(FlushHandler))]
            [InlineData("http://host/RRContent/flushfailures", true, typeof(FlushHandler))]
            [InlineData("http://host/RRContent/flush/9879879879879987", false, null)]
            [InlineData("http://host/RRContent/flushfailures", false, null)]
            [InlineData("http://host/RRContent/2a24329d1c2973c42028f780dbf86641-e8eb6b1157423f3ce5bebd3289395822-RequestReducedStyle.css", false, typeof(ReducedContentHandler))]
            public void WillResolveDefaultMaps(string url, bool postAuth, Type expectedHandler)
            {
                var testable = new TestableHandlerFactory();
                testable.Mock<IUriBuilder>().Setup(
                    x =>
                    x.ParseSignature(
                        "http://host/RRContent/2a24329d1c2973c42028f780dbf86641-e8eb6b1157423f3ce5bebd3289395822-RequestReducedStyle.css"))
                    .Returns(Guid.NewGuid().RemoveDashes());
                RRContainer.Current = new Container(x =>
                {
                    x.For<FlushHandler>().Use(new MoqAutoMocker<FlushHandler>().ClassUnderTest);
                    x.For<DashboardHandler>().Use(new MoqAutoMocker<DashboardHandler>().ClassUnderTest);
                    x.For<ReducedContentHandler>().Use(new MoqAutoMocker<ReducedContentHandler>().ClassUnderTest);
                });

                var result = testable.ClassUnderTest.ResolveHandler(new Uri(url), postAuth);

                if(expectedHandler != null)
                    Assert.IsType(expectedHandler, result);
                else
                    Assert.Null(result);
                RRContainer.Current = null;
            }
        }
    }
}
