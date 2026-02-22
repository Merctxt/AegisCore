using AegisCoreApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace AegisCoreApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class TokenController : ControllerBase
{
    private readonly ITokenService _tokenService;
    private readonly ILogger<TokenController> _logger;
    
    public TokenController(ITokenService tokenService, ILogger<TokenController> logger)
    {
        _tokenService = tokenService;
        _logger = logger;
    }
    
    /// <summary>
    /// Gera um token de acesso para usar a API de moderação
    /// </summary>
    /// <remarks>
    /// - Token válido por 30 minutos
    /// - Limite de 2 tokens ativos por IP
    /// - Use o token no header: X-Access-Token
    /// </remarks>
    [HttpPost("generate")]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> GenerateToken()
    {
        var ipAddress = GetClientIpAddress();
        
        var token = await _tokenService.GenerateTokenAsync(ipAddress);
        
        if (token == null)
        {
            return StatusCode(429, new 
            { 
                error = "Limite de tokens atingido",
                message = "Você já possui 2 tokens ativos. Aguarde a expiração ou use um token existente.",
                limit = 2
            });
        }
        
        _logger.LogInformation("Token gerado para IP {IpAddress}", ipAddress);
        
        return Ok(new TokenResponse(
            Token: token.Token,
            ExpiresAt: token.ExpiresAt,
            ExpiresInMinutes: 30,
            Usage: "Inclua o token no header 'X-Access-Token' para usar a API de moderação"
        ));
    }
    
    /// <summary>
    /// Verifica status de um token
    /// </summary>
    [HttpGet("status")]
    [ProducesResponseType(typeof(TokenStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetTokenStatus([FromHeader(Name = "X-Access-Token")] string? accessToken)
    {
        if (string.IsNullOrEmpty(accessToken))
        {
            return Unauthorized(new { error = "Token não fornecido", message = "Inclua o token no header 'X-Access-Token'" });
        }
        
        var token = await _tokenService.ValidateTokenAsync(accessToken);
        
        if (token == null)
        {
            return Unauthorized(new { error = "Token inválido ou expirado" });
        }
        
        var remainingMinutes = (token.ExpiresAt - DateTime.UtcNow).TotalMinutes;
        
        return Ok(new TokenStatusResponse(
            IsActive: token.IsActive,
            ExpiresAt: token.ExpiresAt,
            RemainingMinutes: Math.Max(0, Math.Round(remainingMinutes, 1)),
            RequestCount: token.RequestCount,
            CreatedAt: token.CreatedAt
        ));
    }
    
    private string GetClientIpAddress()
    {
        // Tenta pegar o IP real (considerando proxies/load balancers)
        var forwardedFor = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',')[0].Trim();
        }
        
        var realIp = HttpContext.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp;
        }
        
        return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}

// DTOs para o Token Controller
public record TokenResponse(
    string Token,
    DateTime ExpiresAt,
    int ExpiresInMinutes,
    string Usage
);

public record TokenStatusResponse(
    bool IsActive,
    DateTime ExpiresAt,
    double RemainingMinutes,
    int RequestCount,
    DateTime CreatedAt
);
