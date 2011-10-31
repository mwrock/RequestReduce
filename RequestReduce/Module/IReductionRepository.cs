using System;
using System.Collections;

namespace RequestReduce.Module
{
    public interface IReductionRepository
    {
        string FindReduction(string urls);
        bool HasLoadedSavedEntries { get; }
        void RemoveReduction(Guid key);
        void AddReduction(Guid key, string reducedUrl);
        string[] ToArray();
    }
}