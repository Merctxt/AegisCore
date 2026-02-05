using System.Text;
using System.Text.Json;
using AegisCoreApi.DTOs;

namespace AegisCoreApi.Services;

public interface IPerspectiveService
{
    Task<ModerationResponse> AnalyzeTextAsync(string text, string language = "pt", bool includeAllScores = false, double? threshold = null);
    Task<BatchModerationResponse> AnalyzeBatchAsync(List<string> texts, string language = "pt", double? threshold = null);
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
    
    private double GetDefaultThreshold()
    {
        var configThreshold = _configuration["Moderation:ToxicityThreshold"];
        return double.TryParse(configThreshold, System.Globalization.NumberStyles.Any, 
            System.Globalization.CultureInfo.InvariantCulture, out var threshold) ? threshold : 0.7;
    }
    
    public async Task<ModerationResponse> AnalyzeTextAsync(string text, string language = "pt", bool includeAllScores = false, double? threshold = null)
    {
        var effectiveThreshold = threshold ?? GetDefaultThreshold();
        var apiKey = _configuration["PerspectiveApi:ApiKey"];
        
        _logger.LogInformation("Threshold: {Threshold}", effectiveThreshold);
        
        if (string.IsNullOrEmpty(apiKey))
        {
            _logger.LogWarning("Perspective API key not configured, using mock response");
            return CreateMockResponse(text, includeAllScores, effectiveThreshold);
        }
        
        _logger.LogInformation("Calling Perspective API for text: {Text}", text);
        
        try
        {
            var requestBody = CreateRequestBody(text, language, includeAllScores);
            var jsonRequest = JsonSerializer.Serialize(requestBody);
            _logger.LogDebug("Request body: {Body}", jsonRequest);
            
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync($"{PerspectiveApiUrl}?key={apiKey}", content);
            var responseContent = await response.Content.ReadAsStringAsync();
            
            _logger.LogInformation("Perspective API response status: {StatusCode}", response.StatusCode);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Perspective API error: {StatusCode} - {Content}", response.StatusCode, responseContent);
                return CreateMockResponse(text, includeAllScores, effectiveThreshold);
            }
            
            _logger.LogDebug("Perspective API response: {Response}", responseContent);
            return ParseResponse(responseContent, text, includeAllScores, effectiveThreshold);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Perspective API");
            return CreateMockResponse(text, includeAllScores, effectiveThreshold);
        }
    }
    
    public async Task<BatchModerationResponse> AnalyzeBatchAsync(List<string> texts, string language = "pt", double? threshold = null)
    {
        var results = new List<ModerationResult>();
        
        foreach (var text in texts)
        {
            var response = await AnalyzeTextAsync(text, language, false, threshold);
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
        var normalizedLanguage = language.ToLower().Split('-')[0];
        
        var attributes = new Dictionary<string, object>
        {
            ["TOXICITY"] = new { }
        };
        
        if (includeAllScores)
        {
            attributes["SEVERE_TOXICITY"] = new { };
            attributes["INSULT"] = new { };
            attributes["PROFANITY"] = new { };
            
            if (normalizedLanguage == "en")
            {
                attributes["IDENTITY_ATTACK"] = new { };
                attributes["THREAT"] = new { };
            }
        }
        
        return new
        {
            comment = new { text },
            languages = new[] { normalizedLanguage },
            requestedAttributes = attributes
        };
    }
    
    private static ModerationResponse ParseResponse(string responseJson, string text, bool includeAllScores, double threshold)
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
                    allScores[attribute] = Math.Round(attrScore.GetProperty("summaryScore").GetProperty("value").GetDouble(), 4);
                }
            }
        }
        
        return new ModerationResponse(
            IsToxic: toxicityScore >= threshold,
            ToxicityScore: Math.Round(toxicityScore, 4),
            ThresholdUsed: threshold,
            AllScores: allScores,
            AnalyzedText: text.Length > 100 ? text[..100] + "..." : text,
            Timestamp: DateTime.UtcNow
        );
    }
    
    private static ModerationResponse CreateMockResponse(string text, bool includeAllScores, double threshold)
    {
        var toxicWords = new[] { "hate", "kill", "stupid", "idiot", "odio", "matar", "idiota", "merda" };
        var lowerText = text.ToLower();
        var hasToxicWord = toxicWords.Any(word => lowerText.Contains(word));
        var score = hasToxicWord ? 0.85 : 0.15;
        
        Dictionary<string, double>? allScores = null;
        if (includeAllScores)
        {
            allScores = new Dictionary<string, double>
            {
                ["TOXICITY"] = score,
                ["SEVERE_TOXICITY"] = score * 0.5,
                ["INSULT"] = score * 0.8,
                ["PROFANITY"] = score * 0.6
            };
        }
        
        return new ModerationResponse(
            IsToxic: score >= threshold,
            ToxicityScore: score,
            ThresholdUsed: threshold,
            AllScores: allScores,
            AnalyzedText: text.Length > 100 ? text[..100] + "..." : text,
            Timestamp: DateTime.UtcNow
        );
    }
}

