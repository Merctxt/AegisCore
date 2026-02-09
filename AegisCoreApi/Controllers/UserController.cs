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
public class UserController : ControllerBase
{
    private readonly AegisDbContext _context;
    private readonly IRequestLogService _logService;
    private readonly ILogger<UserController> _logger;
    
    public UserController(AegisDbContext context, IRequestLogService logService, ILogger<UserController> logger)
    {
        _context = context;
        _logService = logService;
        _logger = logger;
    }
    
    /// <summary>
    /// Get current user profile
    /// </summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProfile()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        
        var user = await _context.Users.FindAsync(userId.Value);
        if (user == null) return NotFound();
        
        var stats = await _logService.GetUsageStatsAsync(userId.Value);
        
        return Ok(new UserResponse(
            user.Id,
            user.Name,
            user.Email,
            user.Plan.ToString(),
            GetDailyLimit(user.Plan),
            stats.RequestsToday,
            user.CreatedAt
        ));
    }
    
    /// <summary>
    /// Get usage statistics
    /// </summary>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(UsageStatsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStats()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        
        var stats = await _logService.GetUsageStatsAsync(userId.Value);
        return Ok(stats);
    }
    
    /// <summary>
    /// Update user profile
    /// </summary>
    [HttpPatch("me")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        
        var user = await _context.Users.FindAsync(userId.Value);
        if (user == null) return NotFound();
        
        if (!string.IsNullOrEmpty(request.Name))
        {
            user.Name = request.Name;
        }
        
        if (!string.IsNullOrEmpty(request.Email))
        {
            // Check if email is already in use
            if (await _context.Users.AnyAsync(u => u.Email == request.Email.ToLower() && u.Id != userId))
            {
                return BadRequest(new { error = "Email already in use" });
            }
            user.Email = request.Email.ToLower();
        }
        
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        
        var stats = await _logService.GetUsageStatsAsync(userId.Value);
        
        return Ok(new UserResponse(
            user.Id,
            user.Name,
            user.Email,
            user.Plan.ToString(),
            GetDailyLimit(user.Plan),
            stats.RequestsToday,
            user.CreatedAt
        ));
    }
    
    /// <summary>
    /// Change password
    /// </summary>
    [HttpPost("change-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        
        var user = await _context.Users.FindAsync(userId.Value);
        if (user == null) return NotFound();
        
        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
        {
            return BadRequest(new { error = "Current password is incorrect" });
        }
        
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        
        return Ok(new { message = "Password changed successfully" });
    }
    
    /// <summary>
    /// Delete user account and all associated data
    /// </summary>
    [HttpDelete("me")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAccount()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();
        
        var user = await _context.Users
            .Include(u => u.ApiKeys)
            .FirstOrDefaultAsync(u => u.Id == userId.Value);
            
        if (user == null) return NotFound();
        
        // Delete all associated data
        _context.ApiKeys.RemoveRange(user.ApiKeys);
        
        // Delete request logs
        var logs = await _context.RequestLogs.Where(l => l.UserId == userId.Value).ToListAsync();
        _context.RequestLogs.RemoveRange(logs);
        
        // Delete user
        _context.Users.Remove(user);
        
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("User account deleted: {UserId}", userId);
        
        return Ok(new { message = "Account deleted successfully" });
    }
    
    private Guid? GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
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
