using RequestReduce.IOC;
using RequestReduce.ResourceTypes;
using Xunit;

namespace RequestReduce.Facts
{
    public class RRContainerFacts
    {
        public class BadResource : IResourceType
        {
            public string FileName
            {
                get { return "badfilename"; }
            }

            public System.Collections.Generic.IEnumerable<string> SupportedMimeTypes
            {
                get { throw new System.NotImplementedException(); }
            }

            public string TransformedMarkupTag(string url)
            {
                throw new System.NotImplementedException();
            }

            public System.Text.RegularExpressions.Regex ResourceRegex
            {
                get { return null; }
            }


            public System.Func<string, string, bool> TagValidator { get; set; }
        }

        [Fact]
        public void ContainerIsValid()
        {
            RRContainer.Current.AssertConfigurationIsValid();
        }

        [Fact]
        public void WillThowIfResourceTypeMissingRequestReduceInFileName()
        {
            RRContainer.Current.Configure(x => x.Scan( y => {
                y.Assembly("RequestReduce.Facts");
                y.AddAllTypesOf<IResourceType>();
            }));

            var ex = Record.Exception(() => RRContainer.Current.AssertConfigurationIsValid());

            Assert.NotNull(ex);
            RRContainer.Current = null;
        }
    }
}
