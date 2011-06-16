using System.DirectoryServices;
using System.IO;
using System.Threading;
using System.Xml.Linq;

namespace RequestReduce.Facts.Integration
{
    internal class IntegrationFactHelper
    {
        public static string ResetPhysicalContentDirectoryAndConfigureStore(Configuration.Store store)
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
                if (!poolRecycled)
                    RecyclePool();
                doc.Save(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName +
                                  "\\RequestReduce.SampleWeb\\web.config");
            }
            return rrFolder;
        }

        public static void RecyclePool()
        {
            using (var pool = new DirectoryEntry("IIS://localhost/W3SVC/AppPools/RequestReduce"))
            {
                pool.Invoke("Recycle", null);
            }
            Thread.Sleep(2000);
        }

    }
}
