using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using RequestReduce.Configuration;
using RequestReduce.IOC;
using RequestReduce.Store;
using RequestReduce.Utilities;

namespace RequestReduce.Module
{
    public class ReductionRepository : IReductionRepository, IDisposable
    {
        private readonly IRRConfiguration configuration;
        protected readonly IDictionary dictionary = new Hashtable();
        private readonly object lockObject = new object();
        protected Timer refresher;

        public ReductionRepository(IRRConfiguration configuration)
        {
            this.configuration = configuration;
            RRTracer.Trace("creating reduction repository");
            refresher = new Timer(LoadDictionaryWithExistingItems, null, 100, configuration.StorePollInterval);
        }

        public void RemoveReduction(Guid key)
        {
            lock (lockObject)
            {
                dictionary.Remove(key);
            }
            RRTracer.Trace("Reduction {0} removed.", key);
        }

        private void LoadDictionaryWithExistingItems(object state)
        {
            try
            {
                RRTracer.Trace("Entering LoadDictionaryWithExistingItems");
                var store = RRContainer.Current.GetInstance<IStore>();
                if (store == null)
                    return;

                var content = store.GetSavedUrls();
                if (content != null)
                {
                    lock(lockObject)
                    {
                        foreach (var pair in content)
                            AddReduction(pair.Key, pair.Value);
                    }
                }
            }
            catch (Exception ex)
            {
                if (RequestReduceModule.CaptureErrorAction != null)
                    RequestReduceModule.CaptureErrorAction(ex);
            }
            finally
            {
                HasLoadedSavedEntries = true;
            }
        }

        public string FindReduction(string urls)
        {
            var hash = Hasher.Hash(urls);
            if (dictionary.Contains(hash))
                return dictionary[hash] as string;

            return null;
        }

        public void AddReduction(Guid key, string reducedUrl)
        {
            try
            {
                lock(lockObject)
                {
                    dictionary[key] = reducedUrl;
                    RRTracer.Trace("Reduction {0} added to ReductionRepository Dictionary.", key);
                }
            }
            catch (ArgumentException)
            {
                
            }
        }

        public bool HasLoadedSavedEntries { get; private set; }

        public void Dispose()
        {
            refresher.Dispose();
        }
    }
}