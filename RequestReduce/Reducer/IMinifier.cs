namespace RequestReduce.Reducer
{
    public interface IMinifier
    {
        string Minify(string unMinifiedContent);
    }
}