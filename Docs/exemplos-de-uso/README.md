⬅️ [Voltar para o índice](../README.md)

# 💡 Exemplos de Uso

Exemplos práticos de integração com a AegisCore API em diversas linguagens de programação.

---

## JavaScript/Node.js

```javascript
const axios = require('axios');

const API_URL = 'https://localhost:5050';
const API_KEY = 'aegis_sua_chave_aqui';

// Análise simples
async function analyzeText(text) {
  try {
    const response = await axios.post(`${API_URL}/api/moderation/analyze`, {
      text: text,
      language: 'pt',
      includeAllScores: true
    }, {
      headers: {
        'X-Api-Key': API_KEY,
        'Content-Type': 'application/json'
      }
    });
    
    const { isToxic, toxicityScore } = response.data;
    
    if (isToxic) {
      console.log(`⚠️ Conteúdo tóxico detectado! Score: ${toxicityScore}`);
    } else {
      console.log('✅ Conteúdo seguro');
    }
    
    return response.data;
  } catch (error) {
    console.error('Erro na análise:', error.message);
    throw error;
  }
}

// Análise em lote
async function analyzeBatch(texts) {
  try {
    const response = await axios.post(`${API_URL}/api/moderation/analyze/batch`, {
      texts: texts,
      language: 'pt'
    }, {
      headers: {
        'X-Api-Key': API_KEY,
        'Content-Type': 'application/json'
      }
    });
    
    return response.data;
  } catch (error) {
    console.error('Erro na análise em lote:', error.message);
    throw error;
  }
}

// Uso
analyzeText("Este é um texto para testar");
analyzeBatch(["Texto 1", "Texto 2", "Texto 3"]);
```

---

## Python

```python
import requests

API_URL = "https://localhost:5050"
API_KEY = "aegis_sua_chave_aqui"

def analyze_text(text: str, language: str = "pt") -> dict:
    """Analisa um texto em busca de conteúdo tóxico."""
    url = f"{API_URL}/api/moderation/analyze"
    headers = {
        "X-Api-Key": API_KEY,
        "Content-Type": "application/json"
    }
    data = {
        "text": text,
        "language": language,
        "includeAllScores": True
    }
    
    response = requests.post(url, json=data, headers=headers)
    response.raise_for_status()
    
    result = response.json()
    
    if result["isToxic"]:
        print(f"⚠️ Conteúdo tóxico! Score: {result['toxicityScore']}")
    else:
        print("✅ Conteúdo seguro")
    
    return result

def analyze_batch(texts: list[str], language: str = "pt") -> dict:
    """Analisa múltiplos textos em uma única requisição."""
    url = f"{API_URL}/api/moderation/analyze/batch"
    headers = {
        "X-Api-Key": API_KEY,
        "Content-Type": "application/json"
    }
    data = {
        "texts": texts,
        "language": language
    }
    
    response = requests.post(url, json=data, headers=headers)
    response.raise_for_status()
    
    return response.json()

# Uso
if __name__ == "__main__":
    analyze_text("Texto para análise")
    analyze_batch(["Texto 1", "Texto 2", "Texto 3"])
```

---

## cURL

```bash
# Análise simples
curl -X POST https://localhost:5050/api/moderation/analyze \
  -H "Content-Type: application/json" \
  -H "X-Api-Key: aegis_sua_chave_aqui" \
  -d '{
    "text": "Texto para analisar",
    "language": "pt"
  }'

# Análise em lote
curl -X POST https://localhost:5050/api/moderation/analyze/batch \
  -H "Content-Type: application/json" \
  -H "X-Api-Key: aegis_sua_chave_aqui" \
  -d '{
    "texts": ["Texto 1", "Texto 2", "Texto 3"],
    "language": "pt"
  }'

# Health check
curl https://localhost:5050/health
```

---

## PHP

```php
<?php

class AegisCoreClient {
    private string $apiUrl;
    private string $apiKey;
    
    public function __construct(string $apiUrl, string $apiKey) {
        $this->apiUrl = $apiUrl;
        $this->apiKey = $apiKey;
    }
    
    public function analyzeText(string $text, string $language = 'pt'): array {
        $url = $this->apiUrl . '/api/moderation/analyze';
        $data = [
            'text' => $text,
            'language' => $language,
            'includeAllScores' => true
        ];
        
        $options = [
            'http' => [
                'header' => implode("\r\n", [
                    "Content-Type: application/json",
                    "X-Api-Key: {$this->apiKey}"
                ]),
                'method' => 'POST',
                'content' => json_encode($data)
            ]
        ];
        
        $context = stream_context_create($options);
        $result = file_get_contents($url, false, $context);
        
        return json_decode($result, true);
    }
}

// Uso
$client = new AegisCoreClient(
    'https://localhost:5050',
    'aegis_sua_chave_aqui'
);

$result = $client->analyzeText("Texto para verificar");

if ($result['isToxic']) {
    echo "⚠️ Conteúdo tóxico detectado!";
} else {
    echo "✅ Conteúdo seguro";
}
?>
```

---

## C# / .NET

```csharp
using System.Net.Http.Json;

public class AegisCoreClient
{
    private readonly HttpClient _client;
    private readonly string _apiKey;

    public AegisCoreClient(string baseUrl, string apiKey)
    {
        _client = new HttpClient { BaseAddress = new Uri(baseUrl) };
        _apiKey = apiKey;
    }

    public async Task<ModerationResult> AnalyzeTextAsync(string text, string language = "pt")
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/moderation/analyze");
        request.Headers.Add("X-Api-Key", _apiKey);
        request.Content = JsonContent.Create(new
        {
            text = text,
            language = language,
            includeAllScores = true
        });

        var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<ModerationResult>();
    }
}

public record ModerationResult(
    bool IsToxic,
    double ToxicityScore,
    string AnalyzedText,
    DateTime Timestamp
);

// Uso
var client = new AegisCoreClient("https://localhost:5050", "aegis_sua_chave_aqui");
var result = await client.AnalyzeTextAsync("Texto para análise");

Console.WriteLine(result.IsToxic ? "⚠️ Tóxico" : "✅ Seguro");
```

---

## TypeScript (com tipos)

```typescript
interface AnalyzeRequest {
  text: string;
  language?: string;
  includeAllScores?: boolean;
}

interface ModerationResult {
  isToxic: boolean;
  toxicityScore: number;
  allScores?: Record<string, number>;
  analyzedText: string;
  timestamp: string;
}

class AegisCoreClient {
  constructor(
    private readonly baseUrl: string,
    private readonly apiKey: string
  ) {}

  async analyze(request: AnalyzeRequest): Promise<ModerationResult> {
    const response = await fetch(`${this.baseUrl}/api/moderation/analyze`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'X-Api-Key': this.apiKey,
      },
      body: JSON.stringify(request),
    });

    if (!response.ok) {
      throw new Error(`API Error: ${response.status}`);
    }

    return response.json();
  }
}

// Uso
const client = new AegisCoreClient('https://localhost:5050', 'aegis_sua_chave');

const result = await client.analyze({
  text: 'Texto para análise',
  language: 'pt',
  includeAllScores: true
});

console.log(result.isToxic ? '⚠️ Tóxico' : '✅ Seguro');
```