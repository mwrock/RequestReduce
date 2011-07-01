using System;
using System.Collections.Concurrent;
using System.Threading;
using RequestReduce.Reducer;
using RequestReduce.Utilities;

namespace RequestReduce.Module
{
    public class ReducingQueue : IReducingQueue
    {
        protected readonly IReductionRepository reductionRepository;
        protected ConcurrentQueue<string> queue = new ConcurrentQueue<string>();
        protected Thread backgroundThread;
        protected bool isRunning = true;
        protected Action<Exception> CaptureErrorAction;

        public ReducingQueue(IReductionRepository reductionRepository)
        {
            RRTracer.Trace("Instantiating new Reducing queue.");
            this.reductionRepository = reductionRepository;
            backgroundThread = new Thread(ProcessQueue) { IsBackground = true };
            backgroundThread.Start();
        }

        public void Enqueue(string urls)
        {
            queue.Enqueue(urls);
        }

        public int Count
        {
            get { return queue.Count; }
        }

        public void CaptureError(Action<Exception> captureAction)
        {
            CaptureErrorAction = captureAction;
        }

        private void ProcessQueue()
        {
            RRTracer.Trace("Process Queue Thread Started");
            while(isRunning)
            {
                ProcessQueuedItem();
                Thread.Sleep(100);
            }
        }

        protected void ProcessQueuedItem()
        {
            try
            {
                string urlsToReduce;
                if (queue.TryDequeue(out urlsToReduce) && reductionRepository.FindReduction(urlsToReduce) == null)
                {
                    var key = Hasher.Hash(urlsToReduce);
                    RRTracer.Trace("dequeued and processing {0}.", urlsToReduce);
                    var reducer = RRContainer.Current.GetInstance<IReducer>();
                    reducer.Process(key, urlsToReduce);
                    RRTracer.Trace("dequeued and processed {0}.", urlsToReduce);
                }
            }
            catch(Exception e)
            {
                RRTracer.Trace(e.ToString());
                if (CaptureErrorAction != null)
                    CaptureErrorAction(e);
            }
        }
    }
}
