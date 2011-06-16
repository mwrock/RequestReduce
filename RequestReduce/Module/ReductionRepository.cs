using System;
using System.Collections;
using System.Threading;
using RequestReduce.Store;
using RequestReduce.Utilities;

namespace RequestReduce.Module
{
    public class ReductionRepository : IReductionRepository
    {
        protected readonly IStore store;
        protected readonly IDictionary dictionary = new Hashtable();
        private object lockObject = new object();

        public ReductionRepository(IStore store)
        {
            this.store = store;
            store.CssAded += AddReduction;
            store.CssDeleted += RemoveReduction;
            var thread = new Thread(LoadDictionaryWithExistingItems);
            thread.IsBackground = true;
            thread.Start();
        }

        private void RemoveReduction(Guid key)
        {
            lock (lockObject)
            {
                dictionary.Remove(key);
            }
            RRTracer.Trace("Reduction {0} removed.", key);
        }

        private void LoadDictionaryWithExistingItems()
        {
            var content = store.GetSavedUrls();
            if(content != null)
            {
                foreach (var pair in content)
                    AddReduction(pair.Key, pair.Value);
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
                    dictionary.Add(key, reducedUrl);
                    RRTracer.Trace("Reduction {0} added to ReductionRepository Dictionary.", key);
                }
            }
            catch (ArgumentException)
            {
                
            }
        }
    }
}