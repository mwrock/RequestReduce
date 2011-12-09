using System.IO;
using System.Net;
using System.Text;
using Xunit;

namespace RequestReduce.Facts.Integration
{
    public class SassLessCoffeeFacts
    {
        [Fact]
        public void WillGetCompilesLessAsCss()
        {
            const string expected = "#header {\n  color: #4d926f;\n}\n";
            string result;
            var client = WebRequest.Create("http://localhost:8877/styles/Style.less");
            var httpResponse = client.GetResponse();

            using (var streameader = new StreamReader(httpResponse.GetResponseStream(), Encoding.UTF8))
            {
                result = streameader.ReadToEnd();
            }

            Assert.Equal(expected, result);
            Assert.Contains("text/css", httpResponse.ContentType);
        }

        [Fact]
        public void WillGetCompilesSassAsCss()
        {
            const string expected = ".content-navigation {\n  border-color: #3bbfce;\n  color: #2ca2af; }\n";
            string result;
            var client = WebRequest.Create("http://localhost:8877/styles/test.sass");
            var httpResponse = client.GetResponse();

            using (var streameader = new StreamReader(httpResponse.GetResponseStream(), Encoding.UTF8))
            {
                result = streameader.ReadToEnd();
            }

            Assert.Equal(expected, result);
            Assert.Contains("text/css", httpResponse.ContentType);
        }

        [Fact]
        public void WillGetCompilesCoffeeAsJs()
        {
            const string expected = "var square;\nsquare = function(x) {\n  return x * x;\n};";
            string result;
            var client = WebRequest.Create("http://localhost:8877/scripts/test.coffee");
            var httpResponse = client.GetResponse();

            using (var streameader = new StreamReader(httpResponse.GetResponseStream(), Encoding.UTF8))
            {
                result = streameader.ReadToEnd();
            }

            Assert.Equal(expected, result);
            Assert.Contains("text/javascript", httpResponse.ContentType);
        }

    }
}
