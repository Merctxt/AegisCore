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
    private readonly ITokenService _tokenService;
    private readonly ILogger<ModerationController> _logger;
    
    public ModerationController(
        IPerspectiveService perspectiveService,
        ITokenService tokenService,
        ILogger<ModerationController> logger)
    {
        _perspectiveService = perspectiveService;
        _tokenService = tokenService;
        _logger = logger;
    }
    
    /// <summary>
    /// Analisa texto para conteúdo tóxico
    /// </summary>
    /// <remarks>
    /// Requer Token no header: X-Access-Token
    /// </remarks>
    [HttpPost("analyze")]
    [ServiceFilter(typeof(TokenAuthFilter))]
    [ProducesResponseType(typeof(ModerationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Analyze([FromBody] ModerationRequest request)
    {
        var stopwatch = Stopwatch.StartNew();
        
        if (!ModelState.IsValid || string.IsNullOrWhiteSpace(request.Text))
        {
            return BadRequest(new { error = "Text is required" });
        }
        
        var accessToken = HttpContext.Items["AccessToken"] as AccessToken;
        
        if (accessToken == null)
        {
            return Unauthorized(new { error = "Token inválido" });
        }
        
        var result = await _perspectiveService.AnalyzeTextAsync(
            request.Text, 
            request.Language ?? "pt", 
            request.IncludeAllScores,
            request.ToxicityThreshold);
        
        stopwatch.Stop();
        
        // Incrementa uso do token
        await _tokenService.IncrementUsageAsync(accessToken.Id);
        
        _logger.LogInformation("Análise realizada em {Ms}ms - Tóxico: {IsToxic}", 
            stopwatch.ElapsedMilliseconds, result.IsToxic);
        
        return Ok(result);
    }
    
    /// <summary>
    /// Analisa múltiplos textos em lote
    /// </summary>
    [HttpPost("analyze/batch")]
    [ServiceFilter(typeof(TokenAuthFilter))]
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
        
        var accessToken = HttpContext.Items["AccessToken"] as AccessToken;
        
        if (accessToken == null)
        {
            return Unauthorized(new { error = "Token inválido" });
        }
        
        var result = await _perspectiveService.AnalyzeBatchAsync(
            request.Texts, 
            request.Language ?? "pt",
            request.ToxicityThreshold);
        
        // Incrementa uso do token pelo número de textos analisados
        for (int i = 0; i < request.Texts.Count; i++)
        {
            await _tokenService.IncrementUsageAsync(accessToken.Id);
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
            version = "2.0.0"
        });
    }
}
