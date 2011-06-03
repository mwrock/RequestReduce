using System;

namespace RequestReduce.Reducer
{
    public interface IReducer : IDisposable
    {
        string Process(Guid key, string urls);
        string Process(string urls);

    }
}