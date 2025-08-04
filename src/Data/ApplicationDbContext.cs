using BackendApi.Users.Models;
using BackendApi.Jokes.Models;
using BackendApi.Topics.Models;
using Microsoft.EntityFrameworkCore;

namespace BackendApi.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<UserModel> Users { get; set; } = null!;
        public DbSet<JokeModel> Jokes { get; set; } = null!;
        public DbSet<TopicModel> Topics { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure unique indexes
            modelBuilder.Entity<UserModel>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<UserModel>()
                .HasIndex(u => u.Name)
                .IsUnique();

            // Configure relationships
            modelBuilder.Entity<JokeModel>()
                .HasOne(j => j.Author)
                .WithMany(u => u.Jokes)
                .HasForeignKey(j => j.AuthorId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete

            // Configure many-to-many relationship between Jokes and Topics
            modelBuilder.Entity<JokeModel>()
                .HasMany(j => j.Topics)
                .WithMany(t => t.Jokes)
                .UsingEntity(j => j.ToTable("JokeTopics"));
        }
    }
}