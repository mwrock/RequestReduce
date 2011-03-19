using System;

namespace RequestReduce.Utilities
{
    public class UrlSplitter : IUrlSplitter
    {
        public string[] Split(string urls)
        {
            return urls.Split(new string[] {"::"}, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}