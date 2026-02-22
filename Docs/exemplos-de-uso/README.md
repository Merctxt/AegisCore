 [Voltar para o índice](../README.md)

# Exemplos de Uso

Exemplos práticos de integração com a AegisCore API em diversas linguagens.

---

## Workflow Básico

1. Gere um token
2. Use o token nas requisições de moderação
3. Quando expirar (30 min), gere outro

---

## JavaScript/Node.js

```javascript
const API_URL = 'https://api.exemplo.com';

// 1. Gerar token
async function getToken() {
  const response = await fetch(`${API_URL}/api/token/generate`, {
    method: 'POST'
  });
  const data = await response.json();
  return data.token;
}

// 2. Analisar texto
async function analyzeText(token, text) {
  const response = await fetch(`${API_URL}/api/moderation/analyze`, {
    method: 'POST',
    headers: {
      'X-Access-Token': token,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({
      text: text,
      language: 'pt',
      includeAllScores: true
    })
  });
  
  return response.json();
}

// 3. Análise em lote
async function analyzeBatch(token, texts) {
  const response = await fetch(`${API_URL}/api/moderation/analyze/batch`, {
    method: 'POST',
    headers: {
      'X-Access-Token': token,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({
      texts: texts,
      language: 'pt'
    })
  });
  
  return response.json();
}

// Uso
async function main() {
  const token = await getToken();
  console.log('Token obtido:', token);
  
  const result = await analyzeText(token, 'Texto para análise');
  console.log(result.isToxic ? '⚠️ Tóxico' : '✅ Seguro');
}

main();
```

---

## Python

```python
import requests

API_URL = "https://api.exemplo.com"

def get_token() -> str:
    """Gera um token de acesso."""
    response = requests.post(f"{API_URL}/api/token/generate")
    response.raise_for_status()
    return response.json()["token"]

def analyze_text(token: str, text: str, language: str = "pt") -> dict:
    """Analisa um texto em busca de conteúdo tóxico."""
    headers = {
        "X-Access-Token": token,
        "Content-Type": "application/json"
    }
    data = {
        "text": text,
        "language": language,
        "includeAllScores": True
    }
    
    response = requests.post(
        f"{API_URL}/api/moderation/analyze",
        json=data,
        headers=headers
    )
    response.raise_for_status()
    return response.json()

def analyze_batch(token: str, texts: list[str], language: str = "pt") -> dict:
    """Analisa múltiplos textos."""
    headers = {
        "X-Access-Token": token,
        "Content-Type": "application/json"
    }
    data = {
        "texts": texts,
        "language": language
    }
    
    response = requests.post(
        f"{API_URL}/api/moderation/analyze/batch",
        json=data,
        headers=headers
    )
    response.raise_for_status()
    return response.json()

# Uso
if __name__ == "__main__":
    token = get_token()
    print(f"Token: {token[:20]}...")
    
    result = analyze_text(token, "Texto para análise")
    print("Tóxico" if result["isToxic"] else "Seguro")
```

---

## cURL

```bash
# Gerar token
TOKEN=$(curl -s -X POST https://api.exemplo.com/api/token/generate | jq -r '.token')
echo "Token: $TOKEN"

# Análise simples
curl -X POST https://api.exemplo.com/api/moderation/analyze \
  -H "Content-Type: application/json" \
  -H "X-Access-Token: $TOKEN" \
  -d '{"text": "Texto para analisar", "language": "pt"}'

# Análise em lote
curl -X POST https://api.exemplo.com/api/moderation/analyze/batch \
  -H "Content-Type: application/json" \
  -H "X-Access-Token: $TOKEN" \
  -d '{"texts": ["Texto 1", "Texto 2", "Texto 3"], "language": "pt"}'

# Verificar status do token
curl -X GET https://api.exemplo.com/api/token/status \
  -H "X-Access-Token: $TOKEN"
```

---

## PHP

```php
<?php

class AegisCoreClient {
    private string $apiUrl;
    private ?string $token = null;
    
    public function __construct(string $apiUrl) {
        $this->apiUrl = $apiUrl;
    }
    
    public function generateToken(): string {
        $ch = curl_init($this->apiUrl . '/api/token/generate');
        curl_setopt($ch, CURLOPT_POST, true);
        curl_setopt($ch, CURLOPT_RETURNTRANSFER, true);
        
        $response = curl_exec($ch);
        curl_close($ch);
        
        $data = json_decode($response, true);
        $this->token = $data['token'];
        return $this->token;
    }
    
    public function analyzeText(string $text, string $language = 'pt'): array {
        if (!$this->token) {
            $this->generateToken();
        }
        
        $ch = curl_init($this->apiUrl . '/api/moderation/analyze');
        curl_setopt($ch, CURLOPT_POST, true);
        curl_setopt($ch, CURLOPT_RETURNTRANSFER, true);
        curl_setopt($ch, CURLOPT_HTTPHEADER, [
            'Content-Type: application/json',
            'X-Access-Token: ' . $this->token
        ]);
        curl_setopt($ch, CURLOPT_POSTFIELDS, json_encode([
            'text' => $text,
            'language' => $language
        ]));
        
        $response = curl_exec($ch);
        curl_close($ch);
        
        return json_decode($response, true);
    }
}

// Uso
$client = new AegisCoreClient('https://api.exemplo.com');
$token = $client->generateToken();
echo "Token: " . substr($token, 0, 20) . "...\n";

$result = $client->analyzeText("Texto para verificar");
echo $result['isToxic'] ? "⚠️ Tóxico\n" : "✅ Seguro\n";
?>
```

---

## C# / .NET

```csharp
using System.Net.Http.Json;

public class AegisCoreClient
{
    private readonly HttpClient _client;
    private string? _token;

    public AegisCoreClient(string baseUrl)
    {
        _client = new HttpClient { BaseAddress = new Uri(baseUrl) };
    }

    public async Task<string> GenerateTokenAsync()
    {
        var response = await _client.PostAsync("/api/token/generate", null);
        response.EnsureSuccessStatusCode();
        
        var data = await response.Content.ReadFromJsonAsync<TokenResponse>();
        _token = data!.Token;
        return _token;
    }

    public async Task<ModerationResult> AnalyzeTextAsync(string text, string language = "pt")
    {
        if (string.IsNullOrEmpty(_token))
            await GenerateTokenAsync();

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/moderation/analyze");
        request.Headers.Add("X-Access-Token", _token);
        request.Content = JsonContent.Create(new { text, language });

        var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<ModerationResult>())!;
    }
}

public record TokenResponse(string Token, DateTime ExpiresAt);
public record ModerationResult(bool IsToxic, double ToxicityScore, DateTime Timestamp);

// Uso
var client = new AegisCoreClient("https://api.exemplo.com");
var token = await client.GenerateTokenAsync();
Console.WriteLine($"Token: {token[..20]}...");

var result = await client.AnalyzeTextAsync("Texto para análise");
Console.WriteLine(result.IsToxic ? "⚠️ Tóxico" : "✅ Seguro");
```

---

## TypeScript

```typescript
interface TokenResponse {
  token: string;
  expiresAt: string;
  expiresInMinutes: number;
}

interface ModerationResult {
  isToxic: boolean;
  toxicityScore: number;
  thresholdUsed: number;
  allScores?: Record<string, number>;
  analyzedText: string;
  timestamp: string;
}

class AegisCoreClient {
  private token?: string;

  constructor(private readonly baseUrl: string) {}

  async generateToken(): Promise<string> {
    const response = await fetch(`${this.baseUrl}/api/token/generate`, {
      method: 'POST'
    });
    
    const data: TokenResponse = await response.json();
    this.token = data.token;
    return this.token;
  }

  async analyze(text: string, language = 'pt'): Promise<ModerationResult> {
    if (!this.token) await this.generateToken();

    const response = await fetch(`${this.baseUrl}/api/moderation/analyze`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'X-Access-Token': this.token!,
      },
      body: JSON.stringify({ text, language, includeAllScores: true }),
    });

    return response.json();
  }
}

// Uso
const client = new AegisCoreClient('https://api.exemplo.com');
const token = await client.generateToken();
console.log(`Token: ${token.slice(0, 20)}...`);

const result = await client.analyze('Texto para análise');
console.log(result.isToxic ? '⚠️ Tóxico' : '✅ Seguro');
```
