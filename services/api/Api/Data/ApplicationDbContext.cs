namespace Api.Data;

using Api.Models;
using Api.Models.Budget;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Application database context.
/// User authentication is delegated to Keycloak, but we maintain a User table
/// for referential integrity and cascade deletion of user-related data.
/// </summary>
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<SubCategory> SubCategories { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // User configuration
        builder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(x => x.UserId);
            entity.Property(x => x.UserId).IsRequired();
        });

        // Category configuration
        builder.Entity<Category>(entity =>
        {
            entity.ToTable("Categories");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).IsRequired();
            entity.HasIndex(x => x.Name).IsUnique();

            entity.HasData(DefaultCategories.GetCategories());
        });

        // SubCategory configuration
        builder.Entity<SubCategory>(entity =>
        {
            entity.ToTable("SubCategories");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).IsRequired();
            entity.HasIndex(x => new { x.Name, x.CategoryId, x.UserId }).IsUnique();

            // Relationship with Category (cascade delete)
            entity.HasOne<Category>()
                  .WithMany(c => c.SubCategories)
                  .HasForeignKey(x => x.CategoryId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Relationship with User (cascade delete when user is deleted)
            // UserId references Users.UserId (which is the Keycloak user ID as Guid)
            entity.HasOne<User>()
                  .WithMany()
                  .HasForeignKey(x => x.UserId)
                  .HasPrincipalKey(u => u.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasData(DefaultCategories.GetSubCategories());
        });
    }
}
