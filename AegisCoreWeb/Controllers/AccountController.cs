using System.Security.Claims;
using AegisCoreWeb.Models;
using AegisCoreWeb.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace AegisCoreWeb.Controllers;

public class AccountController : Controller
{
    private readonly IApiService _apiService;
    private readonly ILogger<AccountController> _logger;
    
    public AccountController(IApiService apiService, ILogger<AccountController> logger)
    {
        _apiService = apiService;
        _logger = logger;
    }
    
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Dashboard");
        }
        
        ViewBag.ReturnUrl = returnUrl;
        return View();
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }
        
        var response = await _apiService.PostAsync<AuthResponse>("api/auth/login", new
        {
            email = model.Email,
            password = model.Password
        });
        
        if (response == null)
        {
            ModelState.AddModelError(string.Empty, "Email ou senha inválidos");
            return View(model);
        }
        
        await SignInUserAsync(response, model.RememberMe);
        
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }
        
        return RedirectToAction("Index", "Dashboard");
    }
    
    [HttpGet]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Dashboard");
        }
        
        return View();
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }
        
        var response = await _apiService.PostAsync<AuthResponse>("api/auth/register", new
        {
            name = model.Name,
            email = model.Email,
            password = model.Password
        });
        
        if (response == null)
        {
            ModelState.AddModelError(string.Empty, "Este email já está em uso");
            return View(model);
        }
        
        await SignInUserAsync(response, true);
        
        TempData["Success"] = "Conta criada com sucesso! Bem-vindo ao AegisCore.";
        return RedirectToAction("Index", "Dashboard");
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        HttpContext.Session.Clear();
        
        return RedirectToAction("Index", "Home");
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
    {
        if (newPassword != confirmPassword)
        {
            TempData["Error"] = "As senhas não coincidem";
            return RedirectToAction("Settings", "Dashboard");
        }
        
        if (newPassword.Length < 8)
        {
            TempData["Error"] = "A nova senha deve ter pelo menos 8 caracteres";
            return RedirectToAction("Settings", "Dashboard");
        }
        
        var token = HttpContext.Session.GetString("Token") ?? User.FindFirst("token")?.Value;
        
        // Usa a rota do UserController que já existe
        var result = await _apiService.PostAsync<object>("api/user/change-password", new
        {
            currentPassword,
            newPassword
        }, token);
        
        if (result != null)
        {
            TempData["Success"] = "Senha alterada com sucesso!";
        }
        else
        {
            TempData["Error"] = "Senha atual incorreta";
        }
        
        
        return RedirectToAction("Settings", "Dashboard");
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAccount()
    {
        var token = HttpContext.Session.GetString("Token") ?? User.FindFirst("token")?.Value;
        
        var result = await _apiService.DeleteAsync("api/user/me", token);
        
        if (result)
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            TempData["Success"] = "Sua conta foi excluída permanentemente";
            return RedirectToAction("Index", "Home");
        }
        
        TempData["Error"] = "Erro ao excluir conta. Tente novamente.";
        return RedirectToAction("Settings", "Dashboard");
    }
    
    private async Task SignInUserAsync(AuthResponse response, bool isPersistent)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, response.User.Id.ToString()),
            new(ClaimTypes.Name, response.User.Name),
            new(ClaimTypes.Email, response.User.Email),
            new("plan", response.User.Plan),
            new("token", response.Token)
        };
        
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = isPersistent,
            ExpiresUtc = response.ExpiresAt
        };
        
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);
        
        // Store token in session
        HttpContext.Session.SetString("Token", response.Token);
    }
}

public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public UserInfo User { get; set; } = new();
}
