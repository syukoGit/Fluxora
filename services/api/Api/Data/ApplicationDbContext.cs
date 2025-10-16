namespace Api.Data;

using Api.Models;
using Api.Models.Budget;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Minimal application database context storing only a link to Keycloak users.
/// </summary>
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<UserAccountLink> UserLinks { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<UserAccountLink>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.KeycloakUserId).IsRequired();
            entity.HasIndex(x => x.KeycloakUserId).IsUnique();
        });

        builder.Entity<Category>(entity =>
        {
            entity.ToTable("Categories");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).IsRequired();
            entity.HasIndex(x => x.Name).IsUnique();

            entity.HasData(DefaultCategories.GetCategories());
        });

        builder.Entity<SubCategory>(entity =>
        {
            entity.ToTable("SubCategories");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).IsRequired();
            entity.HasIndex(x => new { x.Name, x.CategoryId, x.UserId }).IsUnique();
            entity.HasOne<Category>()
                .WithMany(c => c.SubCategories)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<UserAccountLink>()
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasData(DefaultCategories.GetSubCategories());
        });
    }
}
