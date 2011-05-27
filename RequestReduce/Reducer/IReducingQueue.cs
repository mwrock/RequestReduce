using System;

namespace RequestReduce.Reducer
{
    public interface IReducingQueue
    {
        void Enqueue(string urls);
        int Count { get; }
        void CaptureError(Action<Exception> captureAction);
    }
}