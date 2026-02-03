using AegisCoreApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AegisCoreApi.Middleware;

public class ApiKeyAuthFilter : IAsyncActionFilter
{
    private readonly IApiKeyService _apiKeyService;
    private readonly ILogger<ApiKeyAuthFilter> _logger;
    private const string ApiKeyHeader = "X-Api-Key";
    
    public ApiKeyAuthFilter(IApiKeyService apiKeyService, ILogger<ApiKeyAuthFilter> logger)
    {
        _apiKeyService = apiKeyService;
        _logger = logger;
    }
    
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!context.HttpContext.Request.Headers.TryGetValue(ApiKeyHeader, out var apiKeyValue))
        {
            context.Result = new UnauthorizedObjectResult(new 
            { 
                error = "API Key is required",
                message = $"Please provide your API key in the {ApiKeyHeader} header"
            });
            return;
        }
        
        var apiKey = await _apiKeyService.ValidateApiKeyAsync(apiKeyValue.ToString());
        
        if (apiKey == null)
        {
            _logger.LogWarning("Invalid API key attempt: {Key}", apiKeyValue.ToString()[..Math.Min(10, apiKeyValue.ToString().Length)] + "...");
            
            context.Result = new UnauthorizedObjectResult(new 
            { 
                error = "Invalid API Key",
                message = "The provided API key is invalid, expired, or has been revoked"
            });
            return;
        }
        
        // Store the validated API key and user in HttpContext for use in controllers
        context.HttpContext.Items["ApiKey"] = apiKey;
        context.HttpContext.Items["User"] = apiKey.User;
        
        await next();
    }
}
