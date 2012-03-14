using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using RequestReduce.Api;
using RequestReduce.IOC;
using RequestReduce.Reducer;
using RequestReduce.Store;
using RequestReduce.Utilities;

namespace RequestReduce.Module
{
    public class ReducingQueue : IReducingQueue, IDisposable
    {
        public const int FailureThreshold = 5;
        protected readonly IReductionRepository ReductionRepository;
        protected readonly IStore Store;
        private readonly ReaderWriterLockSlim queueLock;
        protected Thread BackgroundThread;
        protected Action<Exception> CaptureErrorAction;
        protected IDictionary<Guid, Failure> DictionaryOfFailures = new Dictionary<Guid, Failure>();
        protected bool IsRunning = true;
        protected Queue<IQueueItem> Queue = new Queue<IQueueItem>();

        public ReducingQueue(IReductionRepository reductionRepository, IStore store)
        {
            RRTracer.Trace("Instantiating new Reducing queue.");
            queueLock = new ReaderWriterLockSlim();
            ReductionRepository = reductionRepository;
            Store = store;
            BackgroundThread = new Thread(ProcessQueue) {IsBackground = true};
            BackgroundThread.Start();
        }

        #region IDisposable Members

        public void Dispose()
        {
            IsRunning = false;
        }

        #endregion

        #region IReducingQueue Members

        public void Enqueue(IQueueItem item)
        {
            queueLock.EnterWriteLock();

            try
            {
                if (!Queue.Any(x => x.Urls == item.Urls) &&
                    (ItemBeingProcessed == null || ItemBeingProcessed.Urls != item.Urls))
                    Queue.Enqueue(item);
            }
            finally
            {
                queueLock.ExitWriteLock();
            }
        }

        public void ClearFailures()
        {
            DictionaryOfFailures.Clear();
        }

        public KeyValuePair<Guid, Failure>[] Failures
        {
            get { return DictionaryOfFailures.ToArray(); }
        }

        public virtual IQueueItem ItemBeingProcessed { get; protected set; }

        public IQueueItem[] ToArray()
        {
            return Queue.ToArray();
        }

        public int Count
        {
            get
            {
                queueLock.EnterReadLock();

                try
                {
                    return Queue.Count;
                }
                finally
                {
                    queueLock.ExitReadLock();
                }
            }
        }

        #endregion

        private void ProcessQueue()
        {
            RRTracer.Trace("Process Queue Thread Started");
            while (IsRunning)
            {
                ProcessQueuedItem();
                Thread.Sleep(100);
            }
        }

        protected void ProcessQueuedItem()
        {
            var key = Guid.Empty;
            IQueueItem itemToReduce = null;
            if (!ReductionRepository.HasLoadedSavedEntries)
                return;
            try
            {
                queueLock.EnterWriteLock();

                try
                {
                    if (Queue.Count > 0)
                        itemToReduce = Queue.Dequeue();
                }
                finally
                {
                    queueLock.ExitWriteLock();
                }

                if (itemToReduce != null &&
                    ReductionRepository.FindReduction(itemToReduce.Urls) == null)
                {
                    ItemBeingProcessed = itemToReduce;
                    key = Hasher.Hash(itemToReduce.Urls);
                    RRTracer.Trace("dequeued and processing {0} with key {1}.", itemToReduce.Urls, key);
                    string url = Store.GetUrlByKey(key, itemToReduce.ResourceType);
                    if (url != null)
                    {
                        RRTracer.Trace("found url {0} in store for key {1}", url, key);
                        ReductionRepository.AddReduction(key, url);
                    }
                    else
                    {
                        if (DictionaryOfFailures.ContainsKey(key) &&
                            DictionaryOfFailures[key].Count >= FailureThreshold)
                        {
                            RRTracer.Trace("{0} has exceeded its failure threshold and will not be processed.",
                                           itemToReduce.Urls);
                            return;
                        }

                        IReducer reducer = RRContainer
                            .Current
                            .GetAllInstances<IReducer>()
                            .SingleOrDefault(x => x.SupportedResourceType == itemToReduce.ResourceType);

                        if (reducer == null)
                        {
                            RRTracer.Trace(string.Format("Failed to retrieve an RRContainer for {0}.",
                                                         itemToReduce.ResourceType));
                            return;
                        }

                        reducer.Process(key, itemToReduce.Urls, itemToReduce.Host);
                    }
                    RRTracer.Trace("dequeued and processed {0}.", itemToReduce.Urls);
                }
            }
            catch (Exception e)
            {
                string message = string.Format("There were errors reducing {0}",
                                               itemToReduce != null ? itemToReduce.Urls : "");
                var wrappedException = new ApplicationException(message, e);
                RRTracer.Trace(message);
                RRTracer.Trace(e.ToString());

                AddFailure(key, wrappedException);

                if (Registry.CaptureErrorAction != null)
                    Registry.CaptureErrorAction(wrappedException);
            }
            finally
            {
                ItemBeingProcessed = null;
            }
        }

        private void AddFailure(Guid key, Exception exception)
        {
            var failure = new Failure
                              {
                                  Guid = key,
                                  Exception =
                                      exception ??
                                      new Exception("No failure was provided (ie. Failure instances was null).")
                              };

            // Check to see we have this failure guid already.
            if (DictionaryOfFailures.ContainsKey(failure.Guid))
            {
                DictionaryOfFailures[failure.Guid].Count++;
            }
            else
            {
                failure.UpdatedOn = DateTime.Now;
                DictionaryOfFailures.Add(failure.Guid, failure);
            }
        }
    }
}