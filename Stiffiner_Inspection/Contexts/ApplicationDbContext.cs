using Microsoft.EntityFrameworkCore;
using Stiffiner_Inspection.Models.Entity;
using System.Data;

namespace Stiffiner_Inspection.Contexts
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Data> Data { get; set; }
        public DbSet<TimeLog> TimeLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

        }
    }
}
