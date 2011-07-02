using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
        protected Dictionary<Guid,int> dictionaryOfFailure = new Dictionary<Guid, int>();
        public const int FailureThreshold = 5;

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

        public void ClearFailures()
        {
            dictionaryOfFailure.Clear();
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
            var key = Guid.Empty;
            try
            {
                string urlsToReduce;
                if (queue.TryDequeue(out urlsToReduce) && reductionRepository.FindReduction(urlsToReduce) == null)
                {
                    key = Hasher.Hash(urlsToReduce);
                    RRTracer.Trace("dequeued and processing {0}.", urlsToReduce);
                    if(dictionaryOfFailure.ContainsKey(key) && dictionaryOfFailure[key] >= FailureThreshold)
                    {
                        RRTracer.Trace("{0} has exceeded its failure threshold and will not be processed.", urlsToReduce);
                        return;
                    }
                    var reducer = RRContainer.Current.GetInstance<IReducer>();
                    reducer.Process(key, urlsToReduce);
                    RRTracer.Trace("dequeued and processed {0}.", urlsToReduce);
                }
            }
            catch(Exception e)
            {
                RRTracer.Trace(e.ToString());
                if(dictionaryOfFailure.ContainsKey(key))
                    dictionaryOfFailure[key] += 1;
                else
                    dictionaryOfFailure.Add(key, 1);
                if (CaptureErrorAction != null)
                    CaptureErrorAction(e);
            }
        }
    }
}
