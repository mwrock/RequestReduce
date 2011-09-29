using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using RequestReduce.IOC;
using RequestReduce.Reducer;
using RequestReduce.Utilities;
using RequestReduce.Store;
using System.Linq;

namespace RequestReduce.Module
{
    public class ReducingQueue : IReducingQueue
    {
        protected readonly IReductionRepository reductionRepository;
        protected readonly IStore store;
        protected ConcurrentQueue<IQueueItem> queue = new ConcurrentQueue<IQueueItem>();
        protected Thread backgroundThread;
        protected bool isRunning = true;
        protected Action<Exception> CaptureErrorAction;
        protected Dictionary<Guid,int> dictionaryOfFailure = new Dictionary<Guid, int>();
        public const int FailureThreshold = 5;

        public ReducingQueue(IReductionRepository reductionRepository, IStore store)
        {
            RRTracer.Trace("Instantiating new Reducing queue.");
            this.reductionRepository = reductionRepository;
            this.store = store;
            backgroundThread = new Thread(ProcessQueue) { IsBackground = true };
            backgroundThread.Start();
        }

        public void Enqueue(IQueueItem item)
        {
            queue.Enqueue(item);
        }

        public void ClearFailures()
        {
            dictionaryOfFailure.Clear();
        }

        public int Count
        {
            get { return queue.Count; }
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
            IQueueItem itemToReduce = null;
            if (!reductionRepository.HasLoadedSavedEntries)
                return;
            try
            {
                if (queue.TryDequeue(out itemToReduce) && reductionRepository.FindReduction(itemToReduce.Urls) == null)
                {
                    key = Hasher.Hash(itemToReduce.Urls);
                    RRTracer.Trace("dequeued and processing {0} with key {1}.", itemToReduce.Urls, key);
                    var url = store.GetUrlByKey(key, itemToReduce.ResourceType);
                    if (url != null)
                    {
                        RRTracer.Trace("found url {0} in store for key {1}", url, key);
                        reductionRepository.AddReduction(key, url);
                    }
                    else
                    {
                        if (dictionaryOfFailure.ContainsKey(key) && dictionaryOfFailure[key] >= FailureThreshold)
                        {
                            RRTracer.Trace("{0} has exceeded its failure threshold and will not be processed.", itemToReduce.Urls);
                            return;
                        }
                        var reducer = RRContainer.Current.GetAllInstances<IReducer>().SingleOrDefault(x => x.SupportedResourceType == itemToReduce.ResourceType);
                        reducer.Process(key, itemToReduce.Urls);
                    }
                    RRTracer.Trace("dequeued and processed {0}.", itemToReduce.Urls);
                }
            }
            catch(Exception e)
            {
                var message = string.Format("There were errors reducing {0}", itemToReduce != null ? itemToReduce.Urls : "");
                var wrappedException =
                    new ApplicationException(message, e);
                RRTracer.Trace(message);
                RRTracer.Trace(e.ToString());
                if(dictionaryOfFailure.ContainsKey(key))
                    dictionaryOfFailure[key] += 1;
                else
                    dictionaryOfFailure.Add(key, 1);
                if (RequestReduceModule.CaptureErrorAction != null)
                    RequestReduceModule.CaptureErrorAction(wrappedException);
            }
        }
    }
}
