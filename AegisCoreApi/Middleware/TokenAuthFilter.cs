using AegisCoreApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AegisCoreApi.Middleware;

public class TokenAuthFilter : IAsyncActionFilter
{
    private readonly ITokenService _tokenService;
    private readonly ILogger<TokenAuthFilter> _logger;
    private const string TokenHeader = "X-Access-Token";
    
    public TokenAuthFilter(ITokenService tokenService, ILogger<TokenAuthFilter> logger)
    {
        _tokenService = tokenService;
        _logger = logger;
    }
    
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!context.HttpContext.Request.Headers.TryGetValue(TokenHeader, out var tokenValue))
        {
            context.Result = new UnauthorizedObjectResult(new 
            { 
                error = "Token de acesso obrigatório",
                message = $"Forneça seu token no header '{TokenHeader}'. Gere um em POST /api/token/generate"
            });
            return;
        }
        
        var token = await _tokenService.ValidateTokenAsync(tokenValue.ToString());
        
        if (token == null)
        {
            _logger.LogWarning("Tentativa de acesso com token inválido: {Token}", 
                tokenValue.ToString()[..Math.Min(10, tokenValue.ToString().Length)] + "...");
            
            context.Result = new UnauthorizedObjectResult(new 
            { 
                error = "Token inválido ou expirado",
                message = "O token fornecido é inválido ou expirou. Gere um novo em POST /api/token/generate"
            });
            return;
        }
        
        // Armazena o token validado no HttpContext para uso nos controllers
        context.HttpContext.Items["AccessToken"] = token;
        
        await next();
    }
}
