using System.IO;
using System.Reflection;

namespace RequestReduce.Resources
{
    internal class ResourceStrings
    {
        private static string dashboard;

        internal static string Dashboard
        {
            get
            {
                if (dashboard==null)
                {
                    using (var myStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("RequestReduce.Resources.Dashboard.html"))
                    {
                        using (var myReader = new StreamReader(myStream))
                        {
                            dashboard = myReader.ReadToEnd();
                        }
                    }
                }
                return dashboard;
            }
        }
    }
}
