namespace RequestReduce.Module
{
    public interface IReductionRepository
    {
        string FindReduction(string urls);
        bool HasLoadedSavedEntries { get; }
    }
}