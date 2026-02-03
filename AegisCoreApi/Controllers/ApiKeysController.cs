using System.Security.Claims;
using AegisCoreApi.Data;
using AegisCoreApi.DTOs;
using AegisCoreApi.Models;
using AegisCoreApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AegisCoreApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class ApiKeysController : ControllerBase
{
    private readonly IApiKeyService _apiKeyService;
    private readonly AegisDbContext _context;
    private readonly ILogger<ApiKeysController> _logger;
    
    public ApiKeysController(IApiKeyService apiKeyService, AegisDbContext context, ILogger<ApiKeysController> logger)
    {
        _apiKeyService = apiKeyService;
        _context = context;
        _logger = logger;
    }
    
    /// <summary>
    /// Get all API keys for the authenticated user
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<ApiKeyResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetApiKeys()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        
        var keys = await _apiKeyService.GetUserApiKeysAsync(userId.Value);
        return Ok(keys);
    }
    
    /// <summary>
    /// Create a new API key
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiKeyCreatedResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateApiKey([FromBody] CreateApiKeyRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        
        // Check key limit (max 5 for free, 10 for paid)
        var user = await _context.Users.FindAsync(userId.Value);
        var keyCount = await _context.ApiKeys.CountAsync(k => k.UserId == userId.Value);
        var maxKeys = user?.Plan == PlanType.Free ? 5 : 10;
        
        if (keyCount >= maxKeys)
        {
            return BadRequest(new { error = $"Maximum {maxKeys} API keys allowed for your plan" });
        }
        
        var result = await _apiKeyService.CreateApiKeyAsync(userId.Value, request);
        return CreatedAtAction(nameof(GetApiKeys), result);
    }
    
    /// <summary>
    /// Revoke an API key (disable it)
    /// </summary>
    [HttpPost("{id}/revoke")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RevokeApiKey(Guid id)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        
        var success = await _apiKeyService.RevokeApiKeyAsync(userId.Value, id);
        
        if (!success)
        {
            return NotFound(new { error = "API key not found" });
        }
        
        return Ok(new { message = "API key revoked successfully" });
    }
    
    /// <summary>
    /// Delete an API key permanently
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteApiKey(Guid id)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        
        var success = await _apiKeyService.DeleteApiKeyAsync(userId.Value, id);
        
        if (!success)
        {
            return NotFound(new { error = "API key not found" });
        }
        
        return NoContent();
    }
    
    private Guid? GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }
}
