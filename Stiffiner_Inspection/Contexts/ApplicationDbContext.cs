using Microsoft.EntityFrameworkCore;
using Stiffiner_Inspection.Models.Entity;

namespace Stiffiner_Inspection.Contexts
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext()
        {
        }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Data> Data { get; set; }
        public DbSet<Image> Images { get; set; }
        public DbSet<Error> Errors { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Error>().HasIndex(x => x.DataId);
            modelBuilder.Entity<Image>().HasIndex(x => x.DataId);

            modelBuilder.Entity<Error>()
                .HasOne(child => child.Data)
                .WithMany(parent => parent.Errors)
                .HasForeignKey(child => child.DataId);

            modelBuilder.Entity<Image>()
                .HasOne(child => child.Data)
                .WithMany(parent => parent.Images)
                .HasForeignKey(child => child.DataId);
        }
    }
}
