using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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

        public string[] GetFiles(string dir)
        {
            return Directory.GetFiles(dir);
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

        public void RenameFile(string originalName, string newName)
        {
            File.Move(originalName, newName);
        }

        public bool FileExists(string path)
        {
            return File.Exists(path);
        }

        public byte[] GetFileBytes(string path)
        {
            return File.ReadAllBytes(path);
        }

        public string GetFileString(string path)
        {
            return File.ReadAllText(path);
        }

		public IList<DatedFileEntry> GetDatedFiles(string directoryPath, string search)
		{
			return new DirectoryInfo(directoryPath).GetFiles(search).Select(x => new DatedFileEntry(x.FullName, x.LastWriteTime)).ToList();	
		}
    }
	
	public struct DatedFileEntry
	{
		public DatedFileEntry(string fileName, DateTime createdDate): this()
		{
			FileName = fileName;
			CreatedDate = createdDate;
		}
		
		public string FileName { get; set; }
		public DateTime CreatedDate { get; set; }
	}
}