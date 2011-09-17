using RequestReduce.IOC;
using Xunit;

namespace RequestReduce.Facts
{
    public class RRContainerFacts
    {
        [Fact]
        public void ContainerIsValid()
        {
            RRContainer.Current.AssertConfigurationIsValid();
        }
    }
}
