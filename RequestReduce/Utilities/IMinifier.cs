using RequestReduce.Reducer;
using System.Security.AccessControl;
using RequestReduce.ResourceTypes;
namespace RequestReduce.Utilities
{
    public interface IMinifier
    {
        string Minify<T>(string unMinifiedContent) where T : IResourceType;
    }
}