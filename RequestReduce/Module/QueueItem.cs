using System;
using RequestReduce.ResourceTypes;

namespace RequestReduce.Module
{
    public class QueueItem<T> : IQueueItem where T : IResourceType
    {
        public string Urls { get; set; }

        public string Host { get; set; }

        public Type ResourceType { get { return typeof(T); } }
    }
}
