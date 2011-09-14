using System;
using RequestReduce.Module;

namespace RequestReduce.Reducer
{
    public interface IReducer : IDisposable
    {
        ResourceType SupportedResourceType { get; }
        string Process(Guid key, string urls);
        string Process(string urls);

    }
}