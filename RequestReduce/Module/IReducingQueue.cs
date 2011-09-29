using System;

namespace RequestReduce.Module
{
    public interface IReducingQueue
    {
        void Enqueue(IQueueItem item);
        int Count { get; }
        void ClearFailures();
    }
}