using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using RequestReduce.Api;
using RequestReduce.Configuration;
using RequestReduce.IOC;
using RequestReduce.Store;
using RequestReduce.Utilities;

namespace RequestReduce.Module
{
    public class ReductionRepository : IReductionRepository, IDisposable
    {
        private readonly IRRConfiguration configuration;
        protected IDictionary dictionary = new Hashtable();
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

                lock(lockObject)
                {
                    var content = store.GetSavedUrls();
                    var hash = new Hashtable(content.Count);
                    foreach (var pair in content)
                        hash.Add(pair.Key, pair.Value);
                    dictionary = hash;
                }
            }
            catch (Exception ex)
            {
                if (Registry.CaptureErrorAction != null)
                    Registry.CaptureErrorAction(ex);
            }
            finally
            {
                HasLoadedSavedEntries = true;
                if (configuration.StorePollInterval == Timeout.Infinite && refresher != null)
                    refresher.Dispose();
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

        public string[] ToArray()
        {
            lock (lockObject)
            {
                return dictionary.Values.Cast<string>().ToArray();
            }
        }

        public bool HasLoadedSavedEntries { get; private set; }

        public void Dispose()
        {
            refresher.Dispose();
            var store = RRContainer.Current.TryGetInstance<IStore>();
            if(store != null)
                store.Dispose();
        }
    }
}