using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AegisCoreApi.Data;
using AegisCoreApi.DTOs;
using AegisCoreApi.Models;
using Microsoft.EntityFrameworkCore;

namespace AegisCoreApi.Services;

public interface IWebhookService
{
    Task<WebhookResponse> CreateWebhookAsync(Guid userId, CreateWebhookRequest request);
    Task<List<WebhookResponse>> GetUserWebhooksAsync(Guid userId);
    Task<bool> DeleteWebhookAsync(Guid userId, Guid webhookId);
    Task TriggerWebhooksAsync(Guid userId, WebhookEventType eventType, object payload);
}

public class WebhookService : IWebhookService
{
    private readonly AegisDbContext _context;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<WebhookService> _logger;
    
    public WebhookService(AegisDbContext context, IHttpClientFactory httpClientFactory, ILogger<WebhookService> logger)
    {
        _context = context;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }
    
    public async Task<WebhookResponse> CreateWebhookAsync(Guid userId, CreateWebhookRequest request)
    {
        var webhook = new Webhook
        {
            UserId = userId,
            Name = request.Name,
            Url = request.Url,
            Secret = request.Secret,
            Events = (WebhookEventType)request.Events
        };
        
        _context.Webhooks.Add(webhook);
        await _context.SaveChangesAsync();
        
        return MapToResponse(webhook);
    }
    
    public async Task<List<WebhookResponse>> GetUserWebhooksAsync(Guid userId)
    {
        var webhooks = await _context.Webhooks
            .Where(w => w.UserId == userId)
            .OrderByDescending(w => w.CreatedAt)
            .ToListAsync();
            
        return webhooks.Select(MapToResponse).ToList();
    }
    
    public async Task<bool> DeleteWebhookAsync(Guid userId, Guid webhookId)
    {
        var webhook = await _context.Webhooks
            .FirstOrDefaultAsync(w => w.Id == webhookId && w.UserId == userId);
            
        if (webhook == null) return false;
        
        _context.Webhooks.Remove(webhook);
        await _context.SaveChangesAsync();
        return true;
    }
    
    public async Task TriggerWebhooksAsync(Guid userId, WebhookEventType eventType, object payload)
    {
        var webhooks = await _context.Webhooks
            .Where(w => w.UserId == userId && w.IsActive && (w.Events & eventType) == eventType)
            .ToListAsync();
            
        foreach (var webhook in webhooks)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await SendWebhookAsync(webhook, eventType, payload);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send webhook {WebhookId}", webhook.Id);
                }
            });
        }
    }
    
    private async Task SendWebhookAsync(Webhook webhook, WebhookEventType eventType, object payload)
    {
        var client = _httpClientFactory.CreateClient();
        client.Timeout = TimeSpan.FromSeconds(10);
        
        var webhookPayload = new
        {
            @event = eventType.ToString(),
            timestamp = DateTime.UtcNow,
            data = payload
        };
        
        var json = JsonSerializer.Serialize(webhookPayload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        // Add signature if secret is configured
        if (!string.IsNullOrEmpty(webhook.Secret))
        {
            var signature = ComputeHmacSha256(json, webhook.Secret);
            content.Headers.Add("X-Aegis-Signature", signature);
        }
        
        content.Headers.Add("X-Aegis-Event", eventType.ToString());
        
        try
        {
            var response = await client.PostAsync(webhook.Url, content);
            
            webhook.LastTriggeredAt = DateTime.UtcNow;
            
            if (!response.IsSuccessStatusCode)
            {
                webhook.FailureCount++;
                
                // Disable webhook after 10 consecutive failures
                if (webhook.FailureCount >= 10)
                {
                    webhook.IsActive = false;
                    _logger.LogWarning("Webhook {WebhookId} disabled after 10 failures", webhook.Id);
                }
            }
            else
            {
                webhook.FailureCount = 0;
            }
            
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            webhook.FailureCount++;
            await _context.SaveChangesAsync();
            _logger.LogError(ex, "Error sending webhook to {Url}", webhook.Url);
        }
    }
    
    private static string ComputeHmacSha256(string data, string secret)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        return "sha256=" + Convert.ToHexString(hash).ToLower();
    }
    
    private static WebhookResponse MapToResponse(Webhook webhook)
    {
        return new WebhookResponse(
            webhook.Id,
            webhook.Name,
            webhook.Url,
            webhook.IsActive,
            webhook.Events.ToString(),
            webhook.CreatedAt,
            webhook.LastTriggeredAt,
            webhook.FailureCount
        );
    }
}
