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
            pngQuantLocation = string.Format("{0}\\pngQuant.exe", dllDir);
        }

        public byte[] OptimizePng(byte[] bytes, int compressionLevel, bool imageQuantizationDisabled)
        {
            var scratchFile = string.Format("{0}\\scratch-{1}.png", configuration.SpritePhysicalPath, Hasher.Hash(bytes));
            if (fileWrapper.FileExists(pngQuantLocation) || fileWrapper.FileExists(optiPngLocation))
                fileWrapper.Save(bytes, scratchFile);
            else 
                return bytes;

            if (fileWrapper.FileExists(pngQuantLocation) && !imageQuantizationDisabled)
            {
                var arg = String.Format(@"256 ""{0}""", scratchFile);
                InvokeExecutable(arg, pngQuantLocation);
                fileWrapper.DeleteFile(scratchFile);
                scratchFile = scratchFile.Replace(".png", "-fs8.png");
            }

            if (fileWrapper.FileExists(optiPngLocation))
            {
                var arg = String.Format(@"-o{1} ""{0}""", scratchFile, compressionLevel);
                InvokeExecutable(arg, optiPngLocation);
            }

            var optimizedBytes = fileWrapper.GetFileBytes(scratchFile);
            fileWrapper.DeleteFile(scratchFile);
            return optimizedBytes;
        }

        private void InvokeExecutable(string arguments, string executable)
        {
            var process = new Process
                              {
                                  StartInfo =
                                      {
                                          UseShellExecute = false,
                                          RedirectStandardOutput = true,
                                          CreateNoWindow = true,
                                          FileName = executable,
                                          Arguments = arguments
                                      }
                              };
            process.Start();
            process.StandardOutput.ReadToEnd();
            process.WaitForExit();
        }
    }
}
