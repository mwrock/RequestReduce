using System;

namespace RequestReduce.Module
{
    public interface IReducingQueue
    {
        void EnqueueCss(string urls);
        void EnqueueJavaScript(string urls);
        int Count { get; }
        void ClearFailures();
    }
}