using System.Data.Entity;

namespace RequestReduce.Store
{
    public class RequestReduceContext : DbContext
    {
        public RequestReduceContext(string connectionString) : base(connectionString)
        {
            
        }

        public DbSet<RequestReduceFile> Files { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RequestReduceFile>()
                .Property(s => s.Content)
                .IsRequired();

            modelBuilder.Entity<RequestReduceFile>()
                .Property(s => s.FileName)
                .IsRequired()
                .HasMaxLength(20);

            modelBuilder.Entity<RequestReduceFile>()
                .Property(s => s.LastAccessed)
                .IsRequired()
                .HasColumnType("datetime2");

            modelBuilder.Entity<RequestReduceFile>()
                .Property(s => s.LastUpdated)
                .IsRequired()
                .HasColumnType("datetime2");
        }
    }
}
