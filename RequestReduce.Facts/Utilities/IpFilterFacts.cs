using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RequestReduce.IOC;
using Xunit;
using RequestReduce.Utilities;
using System.Web;
using Moq;
using System.Collections.Specialized;
using RequestReduce.Configuration;

namespace RequestReduce.Facts.Utilities
{
    public class IpFilterFacts : IDisposable
    {
        class TestableIpFilter : Testable<IpFilter>
        {
            public TestableIpFilter()
            {
            }
        }


        [Fact]
        public void WillDetectPublicIP()
        {
            var testable = new TestableIpFilter();
            testable.Mock<HttpContextBase>().Setup(x => x.Request.UserHostAddress).Returns("123.123.123.123");
            testable.Mock<HttpContextBase>().Setup(x => x.Request.ServerVariables).Returns(new NameValueCollection { { "HTTP_X_FORWARDED_FOR", "9.9.9.9" } });

            Assert.Equal(testable.ClassUnderTest.UserIpAddress(testable.Mock<HttpContextBase>().Object), "123.123.123.123");
        }

        [Fact]
        public void WillDetectAnotherPublicIP()
        {
            var testable = new TestableIpFilter();
            testable.Mock<HttpContextBase>().Setup(x => x.Request.UserHostAddress).Returns("103.103.103.103");
            testable.Mock<HttpContextBase>().Setup(x => x.Request.ServerVariables).Returns(new NameValueCollection { { "HTTP_X_FORWARDED_FOR", "9.9.9.9" } });

            Assert.Equal(testable.ClassUnderTest.UserIpAddress(testable.Mock<HttpContextBase>().Object), "103.103.103.103");
        }

        [Fact]
        public void WillDetectPublicIPv6()
        {
            var testable = new TestableIpFilter();
            testable.Mock<HttpContextBase>().Setup(x => x.Request.UserHostAddress).Returns("3780:0:c307:0:2c45:e6a3:81c7:9273");

            Assert.Equal(testable.ClassUnderTest.UserIpAddress(testable.Mock<HttpContextBase>().Object), "3780:0:c307:0:2c45:e6a3:81c7:9273");
        }

        [Fact]
        public void WillDetectPublicIPWhenBehindPrivateProxy()
        {
            var testable = new TestableIpFilter();
            testable.Mock<HttpContextBase>().Setup(x => x.Request.UserHostAddress).Returns("10.0.0.1");
            testable.Mock<HttpContextBase>().Setup(x => x.Request.ServerVariables).Returns(new NameValueCollection { { "HTTP_X_FORWARDED_FOR", "123.123.123.123" } });

            Assert.Equal(testable.ClassUnderTest.UserIpAddress(testable.Mock<HttpContextBase>().Object), "123.123.123.123");
        }

        [Fact]
        public void WillDetectPublicIPv6WhenBehindPrivateProxy()
        {
            var testable = new TestableIpFilter();
            testable.Mock<HttpContextBase>().Setup(x => x.Request.UserHostAddress).Returns("fde4:8263:a63b:2838:0000:0000:0000:0000");
            testable.Mock<HttpContextBase>().Setup(x => x.Request.ServerVariables).Returns(new NameValueCollection { { "HTTP_X_FORWARDED_FOR", "3780:0:c307:0:2c45:e6a3:81c7:9273" } });

            Assert.Equal(testable.ClassUnderTest.UserIpAddress(testable.Mock<HttpContextBase>().Object), "3780:0:c307:0:2c45:e6a3:81c7:9273");
        }

        [Fact]
        public void WillDetectPublicIPWhenBehindTwoPrivateProxies()
        {
            var testable = new TestableIpFilter();
            testable.Mock<HttpContextBase>().Setup(x => x.Request.UserHostAddress).Returns("10.0.0.1");
            testable.Mock<HttpContextBase>().Setup(x => x.Request.ServerVariables).Returns(new NameValueCollection { { "HTTP_X_FORWARDED_FOR", "123.123.123.123, 10.0.0.2" } });

            Assert.Equal(testable.ClassUnderTest.UserIpAddress(testable.Mock<HttpContextBase>().Object), "123.123.123.123");
        }

        [Fact]
        public void WillDetectPublicIPv6WhenBehindTwoPrivateProxies()
        {
            var testable = new TestableIpFilter();
            testable.Mock<HttpContextBase>().Setup(x => x.Request.UserHostAddress).Returns("fde4:8263:a63b:2838:0000:0000:0000:0000");
            testable.Mock<HttpContextBase>().Setup(x => x.Request.ServerVariables).Returns(new NameValueCollection { { "HTTP_X_FORWARDED_FOR", "3780:0:c307:0:2c45:e6a3:81c7:9273,fd00:1234:5678:2838:0000:0000:0000:0000" } });

            Assert.Equal(testable.ClassUnderTest.UserIpAddress(testable.Mock<HttpContextBase>().Object), "3780:0:c307:0:2c45:e6a3:81c7:9273");
        }

        [Fact]
        public void WillDetectPublicIPWhenBehindTrustedProxy()
        {
            var testable = new TestableIpFilter();
            testable.Mock<IRRConfiguration>().Setup(x => x.ProxyList).Returns(new[] { "111.111.111.111" });
            testable.Mock<HttpContextBase>().Setup(x => x.Request.UserHostAddress).Returns("111.111.111.111");
            testable.Mock<HttpContextBase>().Setup(x => x.Request.ServerVariables).Returns(new NameValueCollection { { "HTTP_X_FORWARDED_FOR", "123.123.123.123" } });

            Assert.Equal(testable.ClassUnderTest.UserIpAddress(testable.Mock<HttpContextBase>().Object), "123.123.123.123");
        }

        [Fact]
        public void WillDetectPublicIPWhenBehindTrustedProxyWithEquivalentIP()
        {
            var testable = new TestableIpFilter();
            testable.Mock<IRRConfiguration>().Setup(x => x.ProxyList).Returns(new[] { "111.000.000.111" });
            testable.Mock<HttpContextBase>().Setup(x => x.Request.UserHostAddress).Returns("111.0.0.111");
            testable.Mock<HttpContextBase>().Setup(x => x.Request.ServerVariables).Returns(new NameValueCollection { { "HTTP_X_FORWARDED_FOR", "123.123.123.123" } });

            Assert.Equal(testable.ClassUnderTest.UserIpAddress(testable.Mock<HttpContextBase>().Object), "123.123.123.123");
        }

        [Fact]
        public void WillDetectPublicIPv6WhenBehindTrustedProxy()
        {
            var testable = new TestableIpFilter();
            testable.Mock<IRRConfiguration>().Setup(x => x.ProxyList).Returns(new[] { "4488:0:5522:0:2c45:e6a3:81c7:9273" });
            testable.Mock<HttpContextBase>().Setup(x => x.Request.UserHostAddress).Returns("4488:0:5522:0:2c45:e6a3:81c7:9273");
            testable.Mock<HttpContextBase>().Setup(x => x.Request.ServerVariables).Returns(new NameValueCollection { { "HTTP_X_FORWARDED_FOR", "3780:0:c307:0:2c45:e6a3:81c7:9273" } });

            Assert.Equal(testable.ClassUnderTest.UserIpAddress(testable.Mock<HttpContextBase>().Object), "3780:0:c307:0:2c45:e6a3:81c7:9273");
        }

        [Fact]
        public void WillDetectPublicIPv6WhenBehindTrustedProxyWithEquivalentIP()
        {
            var testable = new TestableIpFilter();
            testable.Mock<IRRConfiguration>().Setup(x => x.ProxyList).Returns(new[] { "4488:0:5522:0:2c45:e6a3:81c7:9273" });
            testable.Mock<HttpContextBase>().Setup(x => x.Request.UserHostAddress).Returns("4488:0000:5522:0000:2c45:e6a3:81c7:9273");
            testable.Mock<HttpContextBase>().Setup(x => x.Request.ServerVariables).Returns(new NameValueCollection { { "HTTP_X_FORWARDED_FOR", "3780:0:c307:0:2c45:e6a3:81c7:9273" } });

            Assert.Equal(testable.ClassUnderTest.UserIpAddress(testable.Mock<HttpContextBase>().Object), "3780:0:c307:0:2c45:e6a3:81c7:9273");
        }

        [Fact]
        public void WillDetectPrivateIPWithinPrivateNetwork()
        {
            var testable = new TestableIpFilter();
            testable.Mock<HttpContextBase>().Setup(x => x.Request.UserHostAddress).Returns("192.168.0.1");
            testable.Mock<HttpContextBase>().Setup(x => x.Request.ServerVariables).Returns(new NameValueCollection { });

            Assert.Equal(testable.ClassUnderTest.UserIpAddress(testable.Mock<HttpContextBase>().Object), "192.168.0.1");
        }

        [Fact]
        public void WillDetectPrivateIPv6WithinPrivateNetwork()
        {
            var testable = new TestableIpFilter();
            testable.Mock<HttpContextBase>().Setup(x => x.Request.UserHostAddress).Returns("fde4:8263:a63b:2838:0000:0000:0000:0000");
            testable.Mock<HttpContextBase>().Setup(x => x.Request.ServerVariables).Returns(new NameValueCollection { });

            Assert.Equal(testable.ClassUnderTest.UserIpAddress(testable.Mock<HttpContextBase>().Object), "fde4:8263:a63b:2838:0000:0000:0000:0000");
        }

        public void Dispose()
        {
            RRContainer.Current = null;
        }

    }
}
