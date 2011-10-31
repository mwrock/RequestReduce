using System;
using System.Collections.Generic;
namespace RequestReduce.Module
{
    public interface IReducingQueue
    {
        void Enqueue(IQueueItem item);
        int Count { get; }
        void ClearFailures();
        IQueueItem ItemBeingProcessed { get; }
        IQueueItem[] ToArray();
        KeyValuePair<Guid, int>[] Failures { get; }
    }
}