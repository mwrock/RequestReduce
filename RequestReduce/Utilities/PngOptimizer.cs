using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using RequestReduce.Configuration;

namespace RequestReduce.Utilities
{
    public interface IPngOptimizer
    {
        byte[] OptimizePng(byte[] bytes, int compressionLevel, bool imageQuantizationDisabled);
    }

    public class PngOptimizer : IPngOptimizer
    {
        private readonly IFileWrapper fileWrapper;
        private readonly IRRConfiguration configuration;
        private readonly string optiPngLocation;
        private readonly string pngQuantLocation;

        public PngOptimizer(IFileWrapper fileWrapper, IRRConfiguration configuration)
        {
            this.fileWrapper = fileWrapper;
            this.configuration = configuration;
            var dllDir = AppDomain.CurrentDomain.RelativeSearchPath ?? AppDomain.CurrentDomain.BaseDirectory;
            optiPngLocation = string.Format("{0}\\OptiPng.exe", dllDir);
            pngQuantLocation = string.Format("{0}\\pngnqi.exe", dllDir);
        }

        public byte[] OptimizePng(byte[] bytes, int compressionLevel, bool imageQuantizationDisabled)
        {
            var scratchFile = string.Format("{0}\\scratch-{1}.png", configuration.SpritePhysicalPath, Hasher.Hash(bytes));
            try
            {
                if (fileWrapper.FileExists(pngQuantLocation) || fileWrapper.FileExists(optiPngLocation))
                    fileWrapper.Save(bytes, scratchFile);
                else
                    return bytes;

                if (fileWrapper.FileExists(pngQuantLocation) && !imageQuantizationDisabled)
                {
                    var arg = String.Format(@"-Q f -s 1 -g 2.2 -n 256 ""{0}""", scratchFile);
                    InvokeExecutable(arg, pngQuantLocation);
                    fileWrapper.DeleteFile(scratchFile);
                    scratchFile = scratchFile.Replace(".png", "-nq8.png");
                }

                if (fileWrapper.FileExists(optiPngLocation))
                {
                    var arg = String.Format(@"-o{1} ""{0}""", scratchFile, compressionLevel);
                    InvokeExecutable(arg, optiPngLocation);
                }

                var optimizedBytes = fileWrapper.GetFileBytes(scratchFile);
                return optimizedBytes;
            }
            finally
            {
                fileWrapper.DeleteFile(scratchFile);
            }
        }

        private void InvokeExecutable(string arguments, string executable)
        {
            using(var process = new Process())
            {
                process.StartInfo = new ProcessStartInfo()
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    FileName = executable,
                    Arguments = arguments
                };
                process.Start();
                process.StandardOutput.ReadToEnd();
                process.WaitForExit(10000);
                if(!process.HasExited)
                {
                    process.Kill();
                    throw new OptimizationException(string.Format("Unable to optimize image using executable {0} with arguments {1}", executable, arguments));
                }
            }
        }
    }

    public class OptimizationException : Exception
    {
        public OptimizationException(string message) : base(message)
        {
        }
    }
}
