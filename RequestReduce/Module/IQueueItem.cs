using System;
namespace RequestReduce.Module
{
    public interface IQueueItem
    {
        Type ResourceType { get; }
        string Urls { get; set; }
    }
}
