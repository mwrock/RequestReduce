using System;
using RequestReduce.Module;
using RequestReduce.ResourceTypes;

namespace RequestReduce.Reducer
{
    public interface IReducer : IDisposable
    {
        Type SupportedResourceType { get; }
        string Process(Guid key, string urls);
        string Process(string urls);
    }
}