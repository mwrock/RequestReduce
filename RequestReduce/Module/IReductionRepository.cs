namespace RequestReduce.Module
{
    public interface IReductionRepository
    {
        string FindReduction(string urls);
        void AddReduction(string originalUrlList, string reducedUrl);
    }
}