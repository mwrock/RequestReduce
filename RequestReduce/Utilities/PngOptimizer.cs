using System;
using System.Diagnostics;
using System.IO;
using RequestReduce.Configuration;

namespace RequestReduce.Utilities
{
    public interface IPngOptimizer
    {
        byte[] OptimizePng(byte[] bytes, int compressionLevel);
    }

    public class PngOptimizer : IPngOptimizer
    {
        private readonly IFileWrapper fileWrapper;
        private readonly IRRConfiguration configuration;
        private readonly string scratchFile;

        public PngOptimizer(IFileWrapper fileWrapper, IRRConfiguration configuration)
        {
            this.fileWrapper = fileWrapper;
            this.configuration = configuration;
            scratchFile = string.Format(configuration.SpritePhysicalPath + "{0}", "\\scratch.png");
        }

        public byte[] OptimizePng(byte[] bytes, int compressionLevel)
        {
            if (!fileWrapper.FileExists("OptiPng.exe"))
                return bytes;
            fileWrapper.Save(bytes, scratchFile);
            Optimize(compressionLevel, scratchFile);
            var optimizedBytes = fileWrapper.GetFileBytes(scratchFile);
            fileWrapper.DeleteFile(scratchFile);
            return optimizedBytes;
        }

        private void Optimize(int compressionLevel, string filePath)
        {
            var process = new Process();
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.FileName = "optipng.exe";
            process.StartInfo.Arguments = String.Format(
                @"-o{1} ""{0}""",
                filePath, compressionLevel
            );
            process.Start();
            process.StandardOutput.ReadToEnd();
            process.WaitForExit();
        }
    }
}
