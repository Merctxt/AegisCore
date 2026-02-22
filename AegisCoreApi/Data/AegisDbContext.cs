using AegisCoreApi.Models;
using Microsoft.EntityFrameworkCore;

namespace AegisCoreApi.Data;

public class AegisDbContext : DbContext
{
    public AegisDbContext(DbContextOptions<AegisDbContext> options) : base(options)
    {
    }
    
    public DbSet<AccessToken> AccessTokens => Set<AccessToken>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // AccessToken configuration
        modelBuilder.Entity<AccessToken>(entity =>
        {
            entity.HasIndex(t => t.Token).IsUnique();
            entity.HasIndex(t => t.IpAddress);
            entity.HasIndex(t => t.ExpiresAt);
            entity.HasIndex(t => new { t.IpAddress, t.IsActive, t.ExpiresAt });
        });
    }
}
