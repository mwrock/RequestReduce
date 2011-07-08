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
                .Property(s => s.FileName)
                .IsRequired()
                .HasMaxLength(150);

            modelBuilder.Entity<RequestReduceFile>()
                .Property(s => s.IsExpired)
                .IsRequired();

            if (!(base.Database.Connection is System.Data.SqlServerCe.SqlCeConnection))
            {
                modelBuilder.Entity<RequestReduceFile>()
                    .Property(s => s.LastUpdated)
                    .IsRequired()
                    .HasColumnType("datetime2");
                
                modelBuilder.Entity<RequestReduceFile>()
                    .Property(s => s.Content)
                    .IsRequired();

            }
            else
            {
                modelBuilder.Entity<RequestReduceFile>()
                    .Property(s => s.Content)
                    .IsRequired()
                    .HasColumnType("image")
                    .HasMaxLength(null);
                base.Configuration.ValidateOnSaveEnabled = false;
            }
        }
    }
}
