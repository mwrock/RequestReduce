using System.IO;
using StructureMap;

namespace RequestReduce.Utilities
{
    public interface IFileWrapper
    {
        void Save(string content, string fileName);
        void Save(byte[] content, string fileName);
        Stream OpenStream(string fileName);
        bool DirectoryExists(string directoryName);
        void CreateDirectory(string directoryName);
        string[] GetDirectories(string dir);
        void DeleteDirectory(string path);
        void DeleteFile(string path);
    }
}