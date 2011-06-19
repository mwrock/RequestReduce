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
                .HasMaxLength(50);

            modelBuilder.Entity<RequestReduceFile>()
                .Property(s => s.IsExpired)
                .IsRequired();

            if (!(base.Database.Connection is System.Data.SqlServerCe.SqlCeConnection))
            {
                modelBuilder.Entity<RequestReduceFile>()
                    .Property(s => s.LastUpdated)
                    .IsRequired()
                    .HasColumnType("datetime2");
            }
        }
    }
}
