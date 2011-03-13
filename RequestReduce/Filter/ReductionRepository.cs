using System;

namespace RequestReduce.Filter
{
    public interface IReductionRepository
    {
        string FindReduction(string urls);
    }

    public class ReductionRepository : IReductionRepository
    {
        public ReductionRepository()
        {
            
        }
        public string FindReduction(string urls)
        {
            throw new NotImplementedException();
        }
    }
}