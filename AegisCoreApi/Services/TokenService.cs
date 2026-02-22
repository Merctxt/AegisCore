using AegisCoreApi.Data;
using AegisCoreApi.Models;
using Microsoft.EntityFrameworkCore;

namespace AegisCoreApi.Services;

public interface ITokenService
{
    Task<AccessToken?> GenerateTokenAsync(string ipAddress);
    Task<AccessToken?> ValidateTokenAsync(string token);
    Task<int> GetActiveTokenCountByIpAsync(string ipAddress);
    Task DeactivateExpiredTokensAsync();
    Task IncrementUsageAsync(Guid tokenId);
}

public class TokenService : ITokenService
{
    private readonly AegisDbContext _context;
    private readonly ILogger<TokenService> _logger;
    private const int MaxTokensPerIp = 2;
    
    public TokenService(AegisDbContext context, ILogger<TokenService> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    public async Task<AccessToken?> GenerateTokenAsync(string ipAddress)
    {
        // Desativa tokens expirados primeiro
        await DeactivateExpiredTokensAsync();
        
        // Verifica limite de tokens ativos por IP
        var activeCount = await GetActiveTokenCountByIpAsync(ipAddress);
        if (activeCount >= MaxTokensPerIp)
        {
            _logger.LogWarning("IP {IpAddress} atingiu o limite de {Limit} tokens ativos", 
                ipAddress, MaxTokensPerIp);
            return null;
        }
        
        var token = AccessToken.Generate(ipAddress);
        
        _context.AccessTokens.Add(token);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Token gerado para IP {IpAddress}", ipAddress);
        
        return token;
    }
    
    public async Task<AccessToken?> ValidateTokenAsync(string token)
    {
        var accessToken = await _context.AccessTokens
            .FirstOrDefaultAsync(t => t.Token == token && t.IsActive);
            
        if (accessToken == null) return null;
        
        // Verifica se expirou
        if (DateTime.UtcNow >= accessToken.ExpiresAt)
        {
            accessToken.IsActive = false;
            await _context.SaveChangesAsync();
            return null;
        }
        
        return accessToken;
    }
    
    public async Task<int> GetActiveTokenCountByIpAsync(string ipAddress)
    {
        return await _context.AccessTokens
            .CountAsync(t => t.IpAddress == ipAddress && 
                           t.IsActive && 
                           t.ExpiresAt > DateTime.UtcNow);
    }
    
    public async Task DeactivateExpiredTokensAsync()
    {
        var expired = await _context.AccessTokens
            .Where(t => t.IsActive && t.ExpiresAt <= DateTime.UtcNow)
            .ToListAsync();
            
        foreach (var token in expired)
        {
            token.IsActive = false;
        }
        
        if (expired.Count > 0)
        {
            await _context.SaveChangesAsync();
            _logger.LogInformation("Desativados {Count} tokens expirados", expired.Count);
        }
    }
    
    public async Task IncrementUsageAsync(Guid tokenId)
    {
        var token = await _context.AccessTokens.FindAsync(tokenId);
        if (token != null)
        {
            token.RequestCount++;
            token.LastUsedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
}
