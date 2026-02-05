using System.Text;
using System.Text.Json;
using AegisCoreApi.DTOs;

namespace AegisCoreApi.Services;

public interface IPerspectiveService
{
    Task<ModerationResponse> AnalyzeTextAsync(string text, string language = "pt", bool includeAllScores = false);
    Task<BatchModerationResponse> AnalyzeBatchAsync(List<string> texts, string language = "pt");
}

public class PerspectiveService : IPerspectiveService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PerspectiveService> _logger;
    private const string PerspectiveApiUrl = "https://commentanalyzer.googleapis.com/v1alpha1/comments:analyze";
    
    public PerspectiveService(HttpClient httpClient, IConfiguration configuration, ILogger<PerspectiveService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }
    
    public async Task<ModerationResponse> AnalyzeTextAsync(string text, string language = "pt", bool includeAllScores = false)
    {
        var apiKey = _configuration["PerspectiveApi:ApiKey"];
        
        if (string.IsNullOrEmpty(apiKey))
        {
            _logger.LogWarning("Perspective API key not configured, using mock response");
            return CreateMockResponse(text, includeAllScores);
        }
        
        _logger.LogInformation("Calling Perspective API for text analysis");
        
        try
        {
            var requestBody = CreateRequestBody(text, language, includeAllScores);
            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync($"{PerspectiveApiUrl}?key={apiKey}", content);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Perspective API error: {StatusCode} - {Content}", response.StatusCode, errorContent);
                return CreateMockResponse(text, includeAllScores);
            }
            
            var responseJson = await response.Content.ReadAsStringAsync();
            return ParseResponse(responseJson, text, includeAllScores);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Perspective API");
            return CreateMockResponse(text, includeAllScores);
        }
    }
    
    public async Task<BatchModerationResponse> AnalyzeBatchAsync(List<string> texts, string language = "pt")
    {
        var results = new List<ModerationResult>();
        
        foreach (var text in texts)
        {
            var response = await AnalyzeTextAsync(text, language);
            results.Add(new ModerationResult(text, response.IsToxic, response.ToxicityScore));
        }
        
        return new BatchModerationResponse(
            results,
            results.Count,
            results.Count(r => r.IsToxic),
            DateTime.UtcNow
        );
    }
    
    private static object CreateRequestBody(string text, string language, bool includeAllScores)
    {
        var attributes = new Dictionary<string, object>
        {
            ["TOXICITY"] = new { }
        };
        
        if (includeAllScores)
        {
            attributes["SEVERE_TOXICITY"] = new { };
            attributes["IDENTITY_ATTACK"] = new { };
            attributes["INSULT"] = new { };
            attributes["PROFANITY"] = new { };
            attributes["THREAT"] = new { };
        }
        
        return new
        {
            comment = new { text },
            languages = new[] { language },
            requestedAttributes = attributes
        };
    }
    
    private ModerationResponse ParseResponse(string responseJson, string text, bool includeAllScores)
    {
        using var doc = JsonDocument.Parse(responseJson);
        var root = doc.RootElement;
        
        var toxicityScore = root
            .GetProperty("attributeScores")
            .GetProperty("TOXICITY")
            .GetProperty("summaryScore")
            .GetProperty("value")
            .GetDouble();
        
        Dictionary<string, double>? allScores = null;
        
        if (includeAllScores)
        {
            allScores = new Dictionary<string, double>();
            var scores = root.GetProperty("attributeScores");
            
            foreach (var attribute in new[] { "TOXICITY", "SEVERE_TOXICITY", "IDENTITY_ATTACK", "INSULT", "PROFANITY", "THREAT" })
            {
                if (scores.TryGetProperty(attribute, out var attrScore))
                {
                    allScores[attribute] = attrScore.GetProperty("summaryScore").GetProperty("value").GetDouble();
                }
            }
        }
        
        return new ModerationResponse(
            IsToxic: toxicityScore >= 0.7,
            ToxicityScore: Math.Round(toxicityScore, 4),
            AllScores: allScores,
            AnalyzedText: text.Length > 100 ? text[..100] + "..." : text,
            Timestamp: DateTime.UtcNow
        );
    }
    
    private ModerationResponse CreateMockResponse(string text, bool includeAllScores)
    {
        // Simple mock for when API is not configured
        var toxicWords = new[] { "hate", "kill", "stupid", "idiot", "ódio", "matar", "idiota" };
        var lowerText = text.ToLower();
        var isToxic = toxicWords.Any(word => lowerText.Contains(word));
        var score = isToxic ? 0.85 : 0.15;
        
        Dictionary<string, double>? allScores = null;
        if (includeAllScores)
        {
            allScores = new Dictionary<string, double>
            {
                ["TOXICITY"] = score,
                ["SEVERE_TOXICITY"] = score * 0.5,
                ["IDENTITY_ATTACK"] = score * 0.3,
                ["INSULT"] = score * 0.8,
                ["PROFANITY"] = score * 0.6,
                ["THREAT"] = score * 0.2
            };
        }
        
        return new ModerationResponse(
            IsToxic: isToxic,
            ToxicityScore: score,
            AllScores: allScores,
            AnalyzedText: text.Length > 100 ? text[..100] + "..." : text,
            Timestamp: DateTime.UtcNow
        );
    }
}
