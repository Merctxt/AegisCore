using AegisCoreApi.Models;
using Microsoft.EntityFrameworkCore;

namespace AegisCoreApi.Data;

public class AegisDbContext : DbContext
{
    public AegisDbContext(DbContextOptions<AegisDbContext> options) : base(options)
    {
    }
    
    public DbSet<User> Users => Set<User>();
    public DbSet<ApiKey> ApiKeys => Set<ApiKey>();
    public DbSet<RequestLog> RequestLogs => Set<RequestLog>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.Plan).HasConversion<int>();
        });
        
        // ApiKey configuration
        modelBuilder.Entity<ApiKey>(entity =>
        {
            entity.HasIndex(a => a.Key).IsUnique();
            entity.HasIndex(a => a.UserId);
            
            entity.HasOne(a => a.User)
                .WithMany(u => u.ApiKeys)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        // RequestLog configuration
        modelBuilder.Entity<RequestLog>(entity =>
        {
            entity.HasIndex(r => r.CreatedAt);
            entity.HasIndex(r => r.ApiKeyId);
            entity.HasIndex(r => r.UserId);
            
            entity.HasOne(r => r.ApiKey)
                .WithMany(a => a.RequestLogs)
                .HasForeignKey(r => r.ApiKeyId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(r => r.User)
                .WithMany(u => u.RequestLogs)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
