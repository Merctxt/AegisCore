using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace AegisCoreWeb.Services;

public interface IApiService
{
    Task<T?> GetAsync<T>(string endpoint, string? token = null);
    Task<T?> PostAsync<T>(string endpoint, object data, string? token = null);
    Task<T?> PatchAsync<T>(string endpoint, object data, string? token = null);
    Task<bool> DeleteAsync(string endpoint, string? token = null);
}

public class ApiService : IApiService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ApiService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;
    
    public ApiService(IHttpClientFactory httpClientFactory, ILogger<ApiService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }
    
    public async Task<T?> GetAsync<T>(string endpoint, string? token = null)
    {
        try
        {
            var client = CreateClient(token);
            var response = await client.GetAsync(endpoint);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("GET {Endpoint} returned {StatusCode}", endpoint, response.StatusCode);
                return default;
            }
            
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(content, _jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling GET {Endpoint}", endpoint);
            return default;
        }
    }
    
    public async Task<T?> PostAsync<T>(string endpoint, object data, string? token = null)
    {
        try
        {
            var client = CreateClient(token);
            var json = JsonSerializer.Serialize(data, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            _logger.LogInformation("POST {Endpoint} with token: {HasToken}", endpoint, !string.IsNullOrEmpty(token));
            
            var response = await client.PostAsync(endpoint, content);
            var responseContent = await response.Content.ReadAsStringAsync();
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("POST {Endpoint} returned {StatusCode}: {Response}", 
                    endpoint, response.StatusCode, responseContent);
                return default;
            }
            
            return JsonSerializer.Deserialize<T>(responseContent, _jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling POST {Endpoint}", endpoint);
            return default;
        }
    }
    
    public async Task<T?> PatchAsync<T>(string endpoint, object data, string? token = null)
    {
        try
        {
            var client = CreateClient(token);
            var json = JsonSerializer.Serialize(data, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var request = new HttpRequestMessage(HttpMethod.Patch, endpoint) { Content = content };
            var response = await client.SendAsync(request);
            
            if (!response.IsSuccessStatusCode)
            {
                return default;
            }
            
            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(responseContent, _jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling PATCH {Endpoint}", endpoint);
            return default;
        }
    }
    
    public async Task<bool> DeleteAsync(string endpoint, string? token = null)
    {
        try
        {
            var client = CreateClient(token);
            var response = await client.DeleteAsync(endpoint);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling DELETE {Endpoint}", endpoint);
            return false;
        }
    }
    
    private HttpClient CreateClient(string? token)
    {
        var client = _httpClientFactory.CreateClient("AegisApi");
        
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        
        return client;
    }
}
