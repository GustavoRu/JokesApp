using BackendApi.Users.Models;
using Microsoft.EntityFrameworkCore;
// using Users.Models;
namespace BackendApi.Data
{

    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            
        }

        public DbSet<UserModel> Users { get; set; } = null!;

        // protected override void OnModelCreating(ModelBuilder modelBuilder)
        // {
        //     base.OnModelCreating(modelBuilder);

        //     modelBuilder.Entity<UserModel>()
        //         .HasIndex(u => u.Email)
        //         .IsUnique();

        //     modelBuilder.Entity<UserModel>()
        //         .HasIndex(u => u.Username)
        //         .IsUnique();
        // }
    }
}
