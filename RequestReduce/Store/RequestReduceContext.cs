using System.Data.Entity;

namespace RequestReduce.Store
{
    public class RequestReduceContext : DbContext
    {
        public DbSet<RequestReduceFile> Files { get; set; } 
    }
}
