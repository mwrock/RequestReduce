using System.IO;
using StructureMap;

namespace RequestReduce.Utilities
{
    public interface IFileWrapper
    {
        void Save(string content, string fileName);
        void Save(byte[] content, string fileName);
        Stream OpenStream(string fileName);
    }
}