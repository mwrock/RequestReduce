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
    }
}