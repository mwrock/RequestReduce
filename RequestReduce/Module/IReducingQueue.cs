using System;

namespace RequestReduce.Module
{
    public interface IReducingQueue
    {
        void Enqueue(string urls);
        int Count { get; }
        void ClearFailures();
    }
}