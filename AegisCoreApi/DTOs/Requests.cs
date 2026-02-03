using System.ComponentModel.DataAnnotations;

namespace AegisCoreApi.DTOs;

// ========== AUTH DTOs ==========
public record RegisterRequest(
    [Required][MaxLength(100)] string Name,
    [Required][EmailAddress] string Email,
    [Required][MinLength(8)] string Password
);

public record LoginRequest(
    [Required][EmailAddress] string Email,
    [Required] string Password
);

public record AuthResponse(
    string Token,
    DateTime ExpiresAt,
    UserResponse User
);

public record RefreshTokenRequest(string RefreshToken);

// ========== USER DTOs ==========
public record UserResponse(
    Guid Id,
    string Name,
    string Email,
    string Plan,
    int DailyLimit,
    int RequestsToday,
    DateTime CreatedAt
);

public record UpdateUserRequest(
    [MaxLength(100)] string? Name,
    [EmailAddress] string? Email
);

public record ChangePasswordRequest(
    [Required] string CurrentPassword,
    [Required][MinLength(8)] string NewPassword
);

// ========== API KEY DTOs ==========
public record CreateApiKeyRequest(
    [Required][MaxLength(100)] string Name,
    DateTime? ExpiresAt
);

public record ApiKeyResponse(
    Guid Id,
    string Key,
    string Name,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? ExpiresAt,
    DateTime? LastUsedAt,
    int RequestsToday,
    DateTime RequestsResetAt
);

public record ApiKeyCreatedResponse(
    Guid Id,
    string Key,
    string Name,
    DateTime CreatedAt,
    DateTime? ExpiresAt
) 
{
    public string Warning => "Save this key! It won't be shown again.";
}

// ========== MODERATION DTOs ==========
public record ModerationRequest(
    [Required] string Text,
    string? Language = "pt",
    bool IncludeAllScores = false
);

public record ModerationResponse(
    bool IsToxic,
    double ToxicityScore,
    Dictionary<string, double>? AllScores,
    string AnalyzedText,
    DateTime Timestamp
);

public record BatchModerationRequest(
    [Required] List<string> Texts,
    string? Language = "pt"
);

public record BatchModerationResponse(
    List<ModerationResult> Results,
    int TotalAnalyzed,
    int ToxicCount,
    DateTime Timestamp
);

public record ModerationResult(
    string Text,
    bool IsToxic,
    double ToxicityScore
);

// ========== WEBHOOK DTOs ==========
public record CreateWebhookRequest(
    [Required][MaxLength(100)] string Name,
    [Required][Url] string Url,
    string? Secret,
    int Events = 1
);

public record WebhookResponse(
    Guid Id,
    string Name,
    string Url,
    bool IsActive,
    string Events,
    DateTime CreatedAt,
    DateTime? LastTriggeredAt,
    int FailureCount
);

// ========== STATS DTOs ==========
public record UsageStatsResponse(
    int RequestsToday,
    int RequestsThisMonth,
    int DailyLimit,
    double UsagePercentage,
    List<DailyUsage> Last30Days
);

public record DailyUsage(
    DateTime Date,
    int Requests,
    int ToxicDetected
);

// ========== PLAN DTOs ==========
public record PlanInfo(
    string Name,
    string Description,
    int DailyLimit,
    decimal PriceMonthly,
    List<string> Features
);
