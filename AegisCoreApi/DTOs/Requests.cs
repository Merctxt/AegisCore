using System.ComponentModel.DataAnnotations;

namespace AegisCoreApi.DTOs;

// ========== MODERATION DTOs ==========
public record ModerationRequest(
    [Required] string Text,
    string? Language = "pt",
    bool IncludeAllScores = false,
    double? ToxicityThreshold = null
);

public record ModerationResponse(
    bool IsToxic,
    double ToxicityScore,
    double ThresholdUsed,
    Dictionary<string, double>? AllScores,
    string AnalyzedText,
    DateTime Timestamp
);

public record BatchModerationRequest(
    [Required] List<string> Texts,
    string? Language = "pt",
    double? ToxicityThreshold = null
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
