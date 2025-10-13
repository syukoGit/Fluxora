namespace Api.Data;

using Api.Models;
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
    }
}
