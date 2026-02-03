using System.ComponentModel.DataAnnotations;

namespace AegisCoreApi.Models;

public class User
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    public string PasswordHash { get; set; } = string.Empty;
    
    public PlanType Plan { get; set; } = PlanType.Free;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public ICollection<ApiKey> ApiKeys { get; set; } = new List<ApiKey>();
    public ICollection<RequestLog> RequestLogs { get; set; } = new List<RequestLog>();
    public ICollection<Webhook> Webhooks { get; set; } = new List<Webhook>();
}

public enum PlanType
{
    Free = 0,      // 100 requests/day
    Starter = 1,   // 1,000 requests/day
    Pro = 2,       // 10,000 requests/day
    Enterprise = 3 // Unlimited
}
