using System;

namespace RequestReduce.Module
{
    public class Failure
    {
        public Failure()
        {
            Guid = Guid.NewGuid();
            Count = 1;
            CreatedOn = DateTime.Now;
            UpdatedOn = CreatedOn;
        }

        public Guid Guid { get; set; }
        public int Count { get; set; }
        public DateTime CreatedOn { get; private set; }
        public DateTime UpdatedOn { get; set; }
        public Exception Exception { get; set; }
    }
}