namespace RequestReduce.Utilities
{
    public interface IHostingEnvironmentWrapper
    {
        string MapPath(string virtualPath);
    }

    public class HostingEnvironmentWrapper : IHostingEnvironmentWrapper
    {
        public string MapPath(string virtualPath)
        {
            return System.Web.Hosting.HostingEnvironment.MapPath(virtualPath);
        }
    }
}
