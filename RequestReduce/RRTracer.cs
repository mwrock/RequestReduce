using System;
using System.Diagnostics;
using System.Threading;

namespace RequestReduce
{
    public static class RRTracer
    {
        public static void Trace(string messageFormat, params object[] args)
        {
            if (System.Diagnostics.Trace.Listeners.Count <= 0) return;
            var msg = string.Format(messageFormat, args);
            System.Diagnostics.Trace.TraceInformation(string.Format("TIME--{0}::THREAD--{1}/{2}::MSG--{3}",
                                                                    DateTime.Now.TimeOfDay,
                                                                    Thread.CurrentThread.ManagedThreadId, Process.GetCurrentProcess().Id, msg));
        }
    }
}
