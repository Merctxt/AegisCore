using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AegisCoreApi.Models;

public class RequestLog
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    public Guid ApiKeyId { get; set; }
    
    [ForeignKey(nameof(ApiKeyId))]
    public ApiKey ApiKey { get; set; } = null!;
    
    [Required]
    public Guid UserId { get; set; }
    
    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;
    
    [Required]
    [MaxLength(50)]
    public string Endpoint { get; set; } = string.Empty;
    
    [MaxLength(10)]
    public string HttpMethod { get; set; } = "POST";
    
    public int StatusCode { get; set; }
    
    public int ResponseTimeMs { get; set; }
    
    [MaxLength(45)]
    public string? IpAddress { get; set; }
    
    [MaxLength(500)]
    public string? UserAgent { get; set; }
    
    // Moderation result
    public double? ToxicityScore { get; set; }
    
    public bool? IsToxic { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
