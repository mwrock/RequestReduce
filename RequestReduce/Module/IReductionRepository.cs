using System;

namespace RequestReduce.Module
{
    public interface IReductionRepository
    {
        string FindReduction(string urls);
        void AddReduction(Guid key, string reducedUrl);
    }
}