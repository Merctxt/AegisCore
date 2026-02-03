using AegisCoreApi.Data;
using AegisCoreApi.DTOs;
using AegisCoreApi.Models;
using Microsoft.EntityFrameworkCore;

namespace AegisCoreApi.Services;

public interface IApiKeyService
{
    Task<ApiKeyCreatedResponse> CreateApiKeyAsync(Guid userId, CreateApiKeyRequest request);
    Task<List<ApiKeyResponse>> GetUserApiKeysAsync(Guid userId);
    Task<ApiKey?> ValidateApiKeyAsync(string key);
    Task<bool> DeleteApiKeyAsync(Guid userId, Guid keyId);
    Task<bool> RevokeApiKeyAsync(Guid userId, Guid keyId);
    Task IncrementUsageAsync(Guid keyId);
    Task<bool> CheckRateLimitAsync(ApiKey apiKey, User user);
}

public class ApiKeyService : IApiKeyService
{
    private readonly AegisDbContext _context;
    private readonly ILogger<ApiKeyService> _logger;
    
    public ApiKeyService(AegisDbContext context, ILogger<ApiKeyService> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    public async Task<ApiKeyCreatedResponse> CreateApiKeyAsync(Guid userId, CreateApiKeyRequest request)
    {
        var apiKey = new ApiKey
        {
            Key = ApiKey.GenerateKey(),
            Name = request.Name,
            UserId = userId,
            ExpiresAt = request.ExpiresAt
        };
        
        _context.ApiKeys.Add(apiKey);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("API Key created for user {UserId}: {KeyName}", userId, request.Name);
        
        return new ApiKeyCreatedResponse(
            apiKey.Id,
            apiKey.Key,
            apiKey.Name,
            apiKey.CreatedAt,
            apiKey.ExpiresAt
        );
    }
    
    public async Task<List<ApiKeyResponse>> GetUserApiKeysAsync(Guid userId)
    {
        var keys = await _context.ApiKeys
            .Where(k => k.UserId == userId)
            .OrderByDescending(k => k.CreatedAt)
            .ToListAsync();
            
        return keys.Select(k => new ApiKeyResponse(
            k.Id,
            MaskApiKey(k.Key),
            k.Name,
            k.IsActive,
            k.CreatedAt,
            k.ExpiresAt,
            k.LastUsedAt,
            k.RequestsToday,
            k.RequestsResetAt
        )).ToList();
    }
    
    public async Task<ApiKey?> ValidateApiKeyAsync(string key)
    {
        var apiKey = await _context.ApiKeys
            .Include(k => k.User)
            .FirstOrDefaultAsync(k => k.Key == key && k.IsActive);
            
        if (apiKey == null) return null;
        
        // Check if expired
        if (apiKey.ExpiresAt.HasValue && apiKey.ExpiresAt.Value < DateTime.UtcNow)
        {
            return null;
        }
        
        // Reset daily counter if needed
        if (DateTime.UtcNow >= apiKey.RequestsResetAt)
        {
            apiKey.RequestsToday = 0;
            apiKey.RequestsResetAt = DateTime.UtcNow.Date.AddDays(1);
            await _context.SaveChangesAsync();
        }
        
        return apiKey;
    }
    
    public async Task<bool> DeleteApiKeyAsync(Guid userId, Guid keyId)
    {
        var key = await _context.ApiKeys
            .FirstOrDefaultAsync(k => k.Id == keyId && k.UserId == userId);
            
        if (key == null) return false;
        
        _context.ApiKeys.Remove(key);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("API Key deleted: {KeyId}", keyId);
        return true;
    }
    
    public async Task<bool> RevokeApiKeyAsync(Guid userId, Guid keyId)
    {
        var key = await _context.ApiKeys
            .FirstOrDefaultAsync(k => k.Id == keyId && k.UserId == userId);
            
        if (key == null) return false;
        
        key.IsActive = false;
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("API Key revoked: {KeyId}", keyId);
        return true;
    }
    
    public async Task IncrementUsageAsync(Guid keyId)
    {
        var key = await _context.ApiKeys.FindAsync(keyId);
        if (key != null)
        {
            key.RequestsToday++;
            key.LastUsedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
    
    public async Task<bool> CheckRateLimitAsync(ApiKey apiKey, User user)
    {
        var limit = GetDailyLimit(user.Plan);
        
        // Reload to get current count
        await _context.Entry(apiKey).ReloadAsync();
        
        return apiKey.RequestsToday < limit;
    }
    
    private static int GetDailyLimit(PlanType plan) => plan switch
    {
        PlanType.Free => 100,
        PlanType.Starter => 1000,
        PlanType.Pro => 10000,
        PlanType.Enterprise => int.MaxValue,
        _ => 100
    };
    
    private static string MaskApiKey(string key)
    {
        if (key.Length <= 12) return key;
        return key[..8] + "..." + key[^4..];
    }
}
