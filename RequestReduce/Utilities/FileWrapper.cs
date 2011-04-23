using System;
using System.IO;

namespace RequestReduce.Utilities
{
    public class FileWrapper : IFileWrapper
    {
        public void Save(string content, string fileName)
        {
            File.WriteAllText(fileName, content);
        }

        public void Save(byte[] content, string fileName)
        {
            File.WriteAllBytes(fileName, content);
        }

        public Stream OpenStream(string fileName)
        {
            return File.Create(fileName);
        }
    }
}