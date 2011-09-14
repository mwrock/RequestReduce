namespace RequestReduce.Utilities
{
    public interface IMinifier
    {
        string MinifyCss(string unMinifiedContent);
        string MinifyJavaScript(string unMinifiedContent);
    }
}