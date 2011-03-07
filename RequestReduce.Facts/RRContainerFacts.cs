using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
