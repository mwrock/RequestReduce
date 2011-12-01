using System;
using RequestReduce.Utilities;
using Xunit;

namespace RequestReduce.Facts.Utilities
{
    public class HasherFacts
    {
        [Fact]
        public void WillReturnEmptyGuidIfInputStringIsNull()
        {
            var result = Hasher.Hash((string)null);

            Assert.Equal(Guid.Empty, result);
        }
    }
}
