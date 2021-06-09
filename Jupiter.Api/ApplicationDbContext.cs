using Microsoft.EntityFrameworkCore;

namespace Jupiter.Api
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Entity> Entities { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Entity>()
                .HasKey(e => e.Id);
        }
    }
}
