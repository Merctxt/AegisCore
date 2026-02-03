using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AegisCoreApi.Models;

public class Webhook
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    public Guid UserId { get; set; }
    
    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [Url]
    [MaxLength(500)]
    public string Url { get; set; } = string.Empty;
    
    [MaxLength(64)]
    public string? Secret { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public WebhookEventType Events { get; set; } = WebhookEventType.ToxicContent;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? LastTriggeredAt { get; set; }
    
    public int FailureCount { get; set; } = 0;
}

[Flags]
public enum WebhookEventType
{
    None = 0,
    ToxicContent = 1,
    HighToxicity = 2,
    RateLimitReached = 4,
    All = ToxicContent | HighToxicity | RateLimitReached
}
