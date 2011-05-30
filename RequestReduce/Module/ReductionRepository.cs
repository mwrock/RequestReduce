using System;
using System.Collections;
using RequestReduce.Utilities;

namespace RequestReduce.Module
{
    public class ReductionRepository : IReductionRepository
    {
        protected readonly IDictionary dictionary = new Hashtable();

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
                dictionary.Add(key, reducedUrl);
            }
            catch (ArgumentException)
            {
                
            }
        }
    }
}