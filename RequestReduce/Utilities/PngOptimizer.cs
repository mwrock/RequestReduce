using System;
using System.Diagnostics;
using System.IO;
using RequestReduce.Configuration;

namespace RequestReduce.Utilities
{
    public interface IPngOptimizer
    {
        byte[] OptimizePng(byte[] bytes);
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

        public byte[] OptimizePng(byte[] bytes)
        {
            if (!fileWrapper.FileExists("OptiPng.exe"))
                return bytes;
            fileWrapper.Save(bytes, scratchFile);
            Optimize(bytes, scratchFile);
            var optimizedBytes = fileWrapper.GetFileBytes(scratchFile);
            fileWrapper.DeleteFile(scratchFile);
            return optimizedBytes;
        }

        private void Optimize(byte[] bytes, string filePath)
        {
            var process = new Process();
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.FileName = "optipng.exe";
            process.StartInfo.Arguments = String.Format(
                @"-o5 ""{0}""",
                filePath
            );
            process.Start();
            process.StandardOutput.ReadToEnd();
            process.WaitForExit();
        }
    }
}
