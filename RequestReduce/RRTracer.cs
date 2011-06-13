using System;
using System.Threading;

namespace RequestReduce
{
    public delegate void ListenToTraceMessage(string message);
    public static class RRTracer
    {
        public static event ListenToTraceMessage TraceMessageFired;

        public static void Trace(string messageFormat, params object[] args)
        {
            if (TraceMessageFired != null)
            {
                var msg = string.Format(messageFormat, args);
                TraceMessageFired(string.Format("TIME--{0}::THREAD--{1}::MSG--{2}", DateTime.Now.TimeOfDay, Thread.CurrentThread.ManagedThreadId, msg));
            }
        }
    }
}
