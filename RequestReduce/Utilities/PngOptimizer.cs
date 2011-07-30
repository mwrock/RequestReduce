using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
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
        private readonly string optiPngLocation;

        public PngOptimizer(IFileWrapper fileWrapper, IRRConfiguration configuration)
        {
            this.fileWrapper = fileWrapper;
            this.configuration = configuration;
            optiPngLocation = string.Format("{0}\\OptiPng.exe", AppDomain.CurrentDomain.RelativeSearchPath ?? AppDomain.CurrentDomain.BaseDirectory);
        }

        public byte[] OptimizePng(byte[] bytes)
        {
            if (!fileWrapper.FileExists(optiPngLocation))
                return bytes;
            var scratchFile = string.Format("{0}\\scratch-{1}.png", configuration.SpritePhysicalPath, Hasher.Hash(bytes));
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
            process.StartInfo.FileName = optiPngLocation;
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
