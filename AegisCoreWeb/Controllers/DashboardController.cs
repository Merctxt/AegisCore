using System.Security.Claims;
using AegisCoreWeb.Models;
using AegisCoreWeb.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AegisCoreWeb.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly IApiService _apiService;
    private readonly ILogger<DashboardController> _logger;
    
    public DashboardController(IApiService apiService, ILogger<DashboardController> logger)
    {
        _apiService = apiService;
        _logger = logger;
    }
    
    public async Task<IActionResult> Index()
    {
        var token = GetToken();
        
        var model = new DashboardViewModel();
        
        // Get user info
        var user = await _apiService.GetAsync<UserInfo>("api/user/me", token);
        if (user != null) model.User = user;
        
        // Get stats
        var stats = await _apiService.GetAsync<UsageStats>("api/user/stats", token);
        if (stats != null) model.Stats = stats;
        
        // Get API Keys
        var keys = await _apiService.GetAsync<List<ApiKeyInfo>>("api/apikeys", token);
        if (keys != null) model.ApiKeys = keys;
        
        // Get Webhooks
        var webhooks = await _apiService.GetAsync<List<WebhookInfo>>("api/webhooks", token);
        if (webhooks != null) model.Webhooks = webhooks;
        
        return View(model);
    }
    
    public IActionResult ApiKeys()
    {
        return View();
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateApiKey(CreateApiKeyViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Nome da chave é obrigatório";
            return RedirectToAction("Index");
        }
        
        var token = GetToken();
        var result = await _apiService.PostAsync<ApiKeyCreatedResponse>("api/apikeys", new
        {
            name = model.Name,
            expiresAt = model.ExpiresAt
        }, token);
        
        if (result != null)
        {
            TempData["Success"] = $"Chave criada com sucesso!";
            TempData["NewApiKey"] = result.Key;
        }
        else
        {
            TempData["Error"] = "Erro ao criar chave. Verifique se você atingiu o limite do seu plano.";
        }
        
        return RedirectToAction("Index");
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RevokeApiKey(Guid id)
    {
        var token = GetToken();
        await _apiService.PostAsync<object>($"api/apikeys/{id}/revoke", new { }, token);
        
        TempData["Success"] = "Chave revogada com sucesso";
        return RedirectToAction("Index");
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteApiKey(Guid id)
    {
        var token = GetToken();
        await _apiService.DeleteAsync($"api/apikeys/{id}", token);
        
        TempData["Success"] = "Chave excluída com sucesso";
        return RedirectToAction("Index");
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateWebhook(CreateWebhookViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Preencha todos os campos obrigatórios";
            return RedirectToAction("Index");
        }
        
        var token = GetToken();
        var result = await _apiService.PostAsync<WebhookInfo>("api/webhooks", new
        {
            name = model.Name,
            url = model.Url,
            secret = model.Secret,
            events = model.Events
        }, token);
        
        if (result != null)
        {
            TempData["Success"] = "Webhook criado com sucesso!";
        }
        else
        {
            TempData["Error"] = "Erro ao criar webhook";
        }
        
        return RedirectToAction("Index");
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteWebhook(Guid id)
    {
        var token = GetToken();
        await _apiService.DeleteAsync($"api/webhooks/{id}", token);
        
        TempData["Success"] = "Webhook excluído com sucesso";
        return RedirectToAction("Index");
    }
    
    public IActionResult Settings()
    {
        return View();
    }
    
    private string? GetToken()
    {
        return HttpContext.Session.GetString("Token") ?? User.FindFirst("token")?.Value;
    }
}

public class ApiKeyCreatedResponse
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
