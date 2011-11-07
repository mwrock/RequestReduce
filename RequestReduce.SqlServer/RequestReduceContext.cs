using System;
using System.Data.Entity;

namespace RequestReduce.SqlServer
{
    public class RequestReduceContext : DbContext
    {
        public static Type SqlCeType = Type.GetType("System.Data.SqlServerCe.SqlCeConnection, System.Data.SqlServerCe, Version=4.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91", false);

        public RequestReduceContext(string connectionString)
            : base(connectionString)
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

            if (SqlCeType == null || SqlCeType != base.Database.Connection.GetType())
            {
                modelBuilder.Entity<RequestReduceFile>()
                    .Property(s => s.LastUpdated)
                    .IsRequired()
                    .HasColumnType("datetime");

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
                    .IsMaxLength();
                base.Configuration.ValidateOnSaveEnabled = false;
            }
        }
    }
}
