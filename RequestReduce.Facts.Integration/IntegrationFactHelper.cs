using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Xml.Linq;
using Microsoft.Web.Administration;

namespace RequestReduce.Facts.Integration
{
    internal class IntegrationFactHelper
    {
        public static string ResetPhysicalContentDirectoryAndConfigureStore(Configuration.Store store, int poleInterval)
        {
            var poolRecycled = false;
            var rrFolder = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName + "\\RequestReduce.SampleWeb\\RRContent";
            if (Directory.Exists(rrFolder))
            {
                RecyclePool();
                poolRecycled = true;
                try
                {
                    RecyclePool();
                    Directory.Delete(rrFolder, true);
                }
                catch (IOException)
                {
                    Thread.Sleep(100);
                    Directory.Delete(rrFolder, true);
                }
                while (Directory.Exists(rrFolder))
                    Thread.Sleep(0);
            }

            XDocument doc = null;
            using (var stream = File.OpenText(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName +
                              "\\RequestReduce.SampleWeb\\web.config"))
            {
                doc = XDocument.Load(stream);
            }
            var cs = doc.Element("configuration").Element("RequestReduce").Attribute("contentStore");
            if (cs == null || cs.Value.ToLower() != store.ToString().ToLower())
            {
                doc.Element("configuration").Element("RequestReduce").SetAttributeValue("contentStore", store.ToString());
                doc.Element("configuration").Element("RequestReduce").SetAttributeValue("storePollInterval", poleInterval.ToString());
                if (!poolRecycled)
                    RecyclePool();
                doc.Save(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName +
                                  "\\RequestReduce.SampleWeb\\web.config");
            }
            return rrFolder;
        }

        public static void SetSampleWeb35StoreAndTrust(Configuration.Store store, int pollInterval, string trust)
        {
            XDocument doc = null;
            using (var stream = File.OpenText(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName +
                              "\\RequestReduce.SampleWeb35\\web.config"))
            {
                doc = XDocument.Load(stream);
            }
            doc.Element("configuration").Element("RequestReduce").SetAttributeValue("contentStore", store.ToString());
            doc.Element("configuration").Element("RequestReduce").SetAttributeValue("storePollInterval", pollInterval.ToString());
            doc.Element("configuration").Element("system.web").Element("trust").SetAttributeValue("level", trust);
            Recycle35Pool();
            doc.Save(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName +
                              "\\RequestReduce.SampleWeb35\\web.config");
        }

        public static void RecycleNamedPool(string poolName)
        {
            using (var manager = new ServerManager())
            {
                var pool = manager.ApplicationPools[poolName];
                Process process = null;
                if (pool.WorkerProcesses.Count > 0)
                    process = Process.GetProcessById(pool.WorkerProcesses[0].ProcessId);
                pool.Recycle();
                if (process != null)
                {
                    while (!process.HasExited)
                        Thread.Sleep(0);
                    process.Dispose();
                }
            }
        }

        public static void RecyclePool()
        {
            RecycleNamedPool("RequestReduce");
        }

        public static void Recycle35Pool()
        {
            RecycleNamedPool("RequestReduce35");
        }

    }
}
