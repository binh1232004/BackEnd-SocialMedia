using Microsoft.EntityFrameworkCore;
using SocialMedia.Domain.Entities;


namespace SocialMedia.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<User> Users { get; set; } // Đảm bảo có ít nhất 1 bảng

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique(); // Đảm bảo email là duy nhất
        }
    }

}
