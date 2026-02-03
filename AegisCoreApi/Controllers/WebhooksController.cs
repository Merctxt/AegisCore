using System.Security.Claims;
using AegisCoreApi.DTOs;
using AegisCoreApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AegisCoreApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class WebhooksController : ControllerBase
{
    private readonly IWebhookService _webhookService;
    private readonly ILogger<WebhooksController> _logger;
    
    public WebhooksController(IWebhookService webhookService, ILogger<WebhooksController> logger)
    {
        _webhookService = webhookService;
        _logger = logger;
    }
    
    /// <summary>
    /// Get all webhooks for the authenticated user
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<WebhookResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWebhooks()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        
        var webhooks = await _webhookService.GetUserWebhooksAsync(userId.Value);
        return Ok(webhooks);
    }
    
    /// <summary>
    /// Create a new webhook
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(WebhookResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateWebhook([FromBody] CreateWebhookRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        
        var webhook = await _webhookService.CreateWebhookAsync(userId.Value, request);
        return CreatedAtAction(nameof(GetWebhooks), webhook);
    }
    
    /// <summary>
    /// Delete a webhook
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteWebhook(Guid id)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        
        var success = await _webhookService.DeleteWebhookAsync(userId.Value, id);
        
        if (!success)
        {
            return NotFound(new { error = "Webhook not found" });
        }
        
        return NoContent();
    }
    
    private Guid? GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }
}
