using System;
using System.Collections;
using RequestReduce.Reducer;
using RequestReduce.Utilities;

namespace RequestReduce.Filter
{
    public class ReductionRepository : IReductionRepository
    {
        private readonly IDictionary dictionary;
        private readonly IReducingQueue reducingQueue = RRContainer.Current.GetInstance<IReducingQueue>();

        public ReductionRepository(IDictionary dictionary)
        {
            this.dictionary = dictionary;
        }

        public string FindReduction(string urls)
        {
            var hash = Hasher.Hash(urls);
            if (dictionary.Contains(hash))
                return dictionary[hash] as string;

            reducingQueue.Enqueue(urls);
            return null;
        }

        public void AddReduction(string originalUrlList, string reducedUrl)
        {
            throw new NotImplementedException();
        }
    }
}