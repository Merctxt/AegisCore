using System.ComponentModel.DataAnnotations;

namespace AegisCoreApi.Models;

public class AccessToken
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    [MaxLength(64)]
    public string Token { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(45)]
    public string IpAddress { get; set; } = string.Empty;
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime ExpiresAt { get; set; }
    
    public DateTime? LastUsedAt { get; set; }
    
    public int RequestCount { get; set; } = 0;
    
    public static AccessToken Generate(string ipAddress)
    {
        var token = new AccessToken
        {
            IpAddress = ipAddress,
            ExpiresAt = DateTime.UtcNow.AddMinutes(30)
        };
        
        // Gera um token único
        var bytes = new byte[32];
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        token.Token = "aegis_" + Convert.ToBase64String(bytes)
            .Replace("+", "")
            .Replace("/", "")
            .Replace("=", "")[..40];
            
        return token;
    }
}
