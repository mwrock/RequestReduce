using System;

namespace RequestReduce.Utilities
{
    public static class GuidExtensions
    {
        public static string RemoveDashes(this Guid guid)
        {
            return guid.ToString("N");
        }
    }
}
