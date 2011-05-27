using System;
using System.Collections;
using RequestReduce.Reducer;
using RequestReduce.Utilities;

namespace RequestReduce.Filter
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

        public void AddReduction(string originalUrlList, string reducedUrl)
        {
            var hash = Hasher.Hash(originalUrlList);
            try
            {
                dictionary.Add(hash, reducedUrl);
            }
            catch (ArgumentException)
            {
                
            }
        }
    }
}