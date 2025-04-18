using Microsoft.EntityFrameworkCore;

namespace MoviesMinimalAPI.Models
{
    public class MoviesMinimalAPIDbContext : DbContext
    {
        public MoviesMinimalAPIDbContext(DbContextOptions<MoviesMinimalAPIDbContext> options) : base(options)
        {
        }
        public DbSet<Movie> Movies { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Email).HasName("PK__Users__A9D10535B2F51717");

                entity.Property(e => e.Email).HasMaxLength(100);
                entity.Property(e => e.Password).HasMaxLength(500);
                entity.Property(e => e.Role).HasMaxLength(50);
            });

        }
    }
}
