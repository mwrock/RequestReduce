using System;

namespace RequestReduce.Reducer
{
    public interface IReducer
    {
        string Process(Guid key, string urls);
        string Process(string urls);

    }
}