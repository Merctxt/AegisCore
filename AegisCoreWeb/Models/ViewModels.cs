using System.ComponentModel.DataAnnotations;

namespace AegisCoreWeb.Models;

// ========== AUTH ==========
public class LoginViewModel
{
    [Required(ErrorMessage = "Email é obrigatório")]
    [EmailAddress(ErrorMessage = "Email inválido")]
    public string Email { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Senha é obrigatória")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
    
    public bool RememberMe { get; set; }
}

public class RegisterViewModel
{
    [Required(ErrorMessage = "Nome é obrigatório")]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Email é obrigatório")]
    [EmailAddress(ErrorMessage = "Email inválido")]
    public string Email { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Senha é obrigatória")]
    [MinLength(8, ErrorMessage = "Senha deve ter no mínimo 8 caracteres")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Confirmação de senha é obrigatória")]
    [Compare("Password", ErrorMessage = "As senhas não coincidem")]
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = string.Empty;
}

// ========== DASHBOARD ==========
public class DashboardViewModel
{
    public UserInfo User { get; set; } = new();
    public UsageStats Stats { get; set; } = new();
    public List<ApiKeyInfo> ApiKeys { get; set; } = new();
    public List<WebhookInfo> Webhooks { get; set; } = new();
}

public class UserInfo
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Plan { get; set; } = "Free";
    public int DailyLimit { get; set; }
    public int RequestsToday { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class UsageStats
{
    public int RequestsToday { get; set; }
    public int RequestsThisMonth { get; set; }
    public int DailyLimit { get; set; }
    public double UsagePercentage { get; set; }
    public List<DailyUsageItem> Last30Days { get; set; } = new();
}

public class DailyUsageItem
{
    public DateTime Date { get; set; }
    public int Requests { get; set; }
    public int ToxicDetected { get; set; }
}

public class ApiKeyInfo
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime? LastUsedAt { get; set; }
    public int RequestsToday { get; set; }
}

public class WebhookInfo
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string Events { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int FailureCount { get; set; }
}

// ========== API KEY ==========
public class CreateApiKeyViewModel
{
    [Required(ErrorMessage = "Nome é obrigatório")]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    public DateTime? ExpiresAt { get; set; }
}

// ========== WEBHOOK ==========
public class CreateWebhookViewModel
{
    [Required(ErrorMessage = "Nome é obrigatório")]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "URL é obrigatória")]
    [Url(ErrorMessage = "URL inválida")]
    public string Url { get; set; } = string.Empty;
    
    public string? Secret { get; set; }
    
    public int Events { get; set; } = 1;
}

// ========== PLANS ==========
public class PlanViewModel
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int DailyLimit { get; set; }
    public decimal PriceMonthly { get; set; }
    public List<string> Features { get; set; } = new();
    public bool IsCurrentPlan { get; set; }
    public bool IsPopular { get; set; }
}
