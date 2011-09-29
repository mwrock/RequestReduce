using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RequestReduce.Reducer;
using RequestReduce.ResourceTypes;

namespace RequestReduce.Module
{
    public class QueueItem<T> : IQueueItem where T : IResourceType
    {
        public string Urls { get; set; }
        public Type ResourceType { get { return typeof(T); } }
    }
}
