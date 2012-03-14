using System;

namespace RequestReduce.Reducer
{
    public interface IReducer : IDisposable
    {
        Type SupportedResourceType { get; }
        string Process(Guid key, string urls, string host);
        string Process(string urls);
    }
}