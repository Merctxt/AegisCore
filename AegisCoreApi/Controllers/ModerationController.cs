using System.Diagnostics;
using AegisCoreApi.DTOs;
using AegisCoreApi.Middleware;
using AegisCoreApi.Models;
using AegisCoreApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace AegisCoreApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ModerationController : ControllerBase
{
    private readonly IPerspectiveService _perspectiveService;
    private readonly IApiKeyService _apiKeyService;
    private readonly IRequestLogService _logService;
    private readonly IWebhookService _webhookService;
    private readonly ILogger<ModerationController> _logger;
    
    public ModerationController(
        IPerspectiveService perspectiveService,
        IApiKeyService apiKeyService,
        IRequestLogService logService,
        IWebhookService webhookService,
        ILogger<ModerationController> logger)
    {
        _perspectiveService = perspectiveService;
        _apiKeyService = apiKeyService;
        _logService = logService;
        _webhookService = webhookService;
        _logger = logger;
    }
    
    /// <summary>
    /// Analyze text for toxic content
    /// </summary>
    /// <remarks>
    /// Requires API Key in header: X-Api-Key
    /// </remarks>
    [HttpPost("analyze")]
    [ServiceFilter(typeof(ApiKeyAuthFilter))]
    [ProducesResponseType(typeof(ModerationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Analyze([FromBody] ModerationRequest request)
    {
        var stopwatch = Stopwatch.StartNew();
        
        if (!ModelState.IsValid || string.IsNullOrWhiteSpace(request.Text))
        {
            return BadRequest(new { error = "Text is required" });
        }
        
        var apiKey = HttpContext.Items["ApiKey"] as ApiKey;
        var user = HttpContext.Items["User"] as User;
        
        if (apiKey == null || user == null)
        {
            return Unauthorized(new { error = "Invalid API Key" });
        }
        
        // Check rate limit
        if (!await _apiKeyService.CheckRateLimitAsync(apiKey, user))
        {
            // Trigger rate limit webhook
            await _webhookService.TriggerWebhooksAsync(user.Id, WebhookEventType.RateLimitReached, new
            {
                apiKeyId = apiKey.Id,
                apiKeyName = apiKey.Name
            });
            
            return StatusCode(429, new { error = "Daily rate limit exceeded", limit = GetDailyLimit(user.Plan) });
        }
        
        var result = await _perspectiveService.AnalyzeTextAsync(
            request.Text, 
            request.Language ?? "pt", 
            request.IncludeAllScores,
            request.ToxicityThreshold);
        
        stopwatch.Stop();
        
        // Log the request
        await _logService.LogRequestAsync(new RequestLog
        {
            ApiKeyId = apiKey.Id,
            UserId = user.Id,
            Endpoint = "/api/moderation/analyze",
            HttpMethod = "POST",
            StatusCode = 200,
            ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent = Request.Headers.UserAgent.ToString(),
            ToxicityScore = result.ToxicityScore,
            IsToxic = result.IsToxic
        });
        
        // Increment usage
        await _apiKeyService.IncrementUsageAsync(apiKey.Id);
        
        // Trigger webhook if toxic
        if (result.IsToxic)
        {
            var eventType = result.ToxicityScore >= 0.9 
                ? WebhookEventType.HighToxicity 
                : WebhookEventType.ToxicContent;
                
            await _webhookService.TriggerWebhooksAsync(user.Id, eventType, new
            {
                text = request.Text,
                toxicityScore = result.ToxicityScore,
                analyzedAt = result.Timestamp
            });
        }
        
        return Ok(result);
    }
    
    /// <summary>
    /// Analyze multiple texts in batch
    /// </summary>
    [HttpPost("analyze/batch")]
    [ServiceFilter(typeof(ApiKeyAuthFilter))]
    [ProducesResponseType(typeof(BatchModerationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AnalyzeBatch([FromBody] BatchModerationRequest request)
    {
        if (!ModelState.IsValid || request.Texts == null || request.Texts.Count == 0)
        {
            return BadRequest(new { error = "Texts array is required" });
        }
        
        if (request.Texts.Count > 100)
        {
            return BadRequest(new { error = "Maximum 100 texts per batch" });
        }
        
        var apiKey = HttpContext.Items["ApiKey"] as ApiKey;
        var user = HttpContext.Items["User"] as User;
        
        if (apiKey == null || user == null)
        {
            return Unauthorized(new { error = "Invalid API Key" });
        }
        
        
        var result = await _perspectiveService.AnalyzeBatchAsync(
            request.Texts, 
            request.Language ?? "pt",
            request.ToxicityThreshold);
        
        // Increment usage for each text
        for (int i = 0; i < request.Texts.Count; i++)
        {
            await _apiKeyService.IncrementUsageAsync(apiKey.Id);
        }
        
        return Ok(result);
    }
    
    /// <summary>
    /// Health check endpoint
    /// </summary>
    [HttpGet("health")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Health()
    {
        return Ok(new 
        { 
            status = "healthy", 
            timestamp = DateTime.UtcNow,
            version = "1.0.0"
        });
    }
    
    private static int GetDailyLimit(PlanType plan) => plan switch
    {
        PlanType.Free => 100,
        PlanType.Starter => 1000,
        PlanType.Pro => 10000,
        PlanType.Enterprise => int.MaxValue,
        _ => 100
    };
}
