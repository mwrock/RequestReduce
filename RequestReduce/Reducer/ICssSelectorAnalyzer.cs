namespace RequestReduce.Reducer
{
    public interface ICssSelectorAnalyzer
    {
        bool IsInScopeOfTarget(string targetSelector, string comparableSelector);
    }
}