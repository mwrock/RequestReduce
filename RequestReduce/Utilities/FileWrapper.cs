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

        public bool DirectoryExists(string directoryName)
        {
            return Directory.Exists(directoryName);
        }

        public void CreateDirectory(string directoryName)
        {
            Directory.CreateDirectory(directoryName);
        }

        public string[] GetDirectories(string dir)
        {
            return Directory.GetDirectories(dir);
        }

        public void DeleteDirectory(string path)
        {
            if(!string.IsNullOrEmpty(path))
                Directory.Delete(path, true);
        }

        public void DeleteFile(string path)
        {
            File.Delete(path);
        }
    }
}