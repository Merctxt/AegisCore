 [Voltar para o índice](../README.md)

# Códigos de Status HTTP

Esta página documenta todos os códigos de status HTTP retornados pela AegisCore API.

---

## Códigos de Sucesso

| Código | Nome | Descrição |
|--------|------|-----------|
| 200 | OK | Requisição processada com sucesso |

---

## Códigos de Erro do Cliente

| Código | Nome | Descrição |
|--------|------|-----------|
| 400 | Bad Request | Dados inválidos na requisição |
| 401 | Unauthorized | Token inválido, ausente ou expirado |
| 429 | Too Many Requests | Limite de tokens por IP atingido |

---

## Códigos de Erro do Servidor

| Código | Nome | Descrição |
|--------|------|-----------|
| 500 | Internal Server Error | Erro interno do servidor |
| 502 | Bad Gateway | Erro de comunicação com Perspective API |
| 503 | Service Unavailable | Serviço temporariamente indisponível |

---

## Exemplos de Resposta de Erro

### 400 - Bad Request

```json
{
  "error": "Text is required"
}
```

### 401 - Token não fornecido

```json
{
  "error": "Token de acesso obrigatório",
  "message": "Forneça seu token no header 'X-Access-Token'. Gere um em POST /api/token/generate"
}
```

### 401 - Token inválido ou expirado

```json
{
  "error": "Token inválido ou expirado",
  "message": "O token fornecido é inválido ou expirou. Gere um novo em POST /api/token/generate"
}
```

### 429 - Limite de tokens atingido

```json
{
  "error": "Limite de tokens atingido",
  "message": "Você já possui 2 tokens ativos. Aguarde a expiração ou use um token existente.",
  "limit": 2
}
```

### 500 - Erro interno

```json
{
  "error": "Internal Server Error",
  "message": "Ocorreu um erro inesperado. Tente novamente."
}
```

---

## Tratando Erros

### JavaScript

```javascript
async function analyzeWithErrorHandling(token, text) {
  try {
    const response = await fetch('/api/moderation/analyze', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'X-Access-Token': token
      },
      body: JSON.stringify({ text })
    });

    if (!response.ok) {
      const error = await response.json();
      
      switch (response.status) {
        case 400:
          console.error('Dados inválidos:', error.error);
          break;
        case 401:
          console.error('Token expirado, gerando novo...');
          // Gerar novo token
          break;
        case 429:
          console.error('Limite atingido, aguarde expiração');
          break;
        default:
          console.error('Erro:', error.message);
      }
      return null;
    }

    return await response.json();
  } catch (error) {
    console.error('Erro de rede:', error);
    return null;
  }
}
```

### Python

```python
import requests
from requests.exceptions import HTTPError

def analyze_with_error_handling(token: str, text: str):
    try:
        response = requests.post(
            'https://api.exemplo.com/api/moderation/analyze',
            json={'text': text},
            headers={'X-Access-Token': token}
        )
        response.raise_for_status()
        return response.json()

    except HTTPError as e:
        status = e.response.status_code
        error = e.response.json()
        
        if status == 401:
            print('Token expirado, gere um novo')
        elif status == 429:
            print('Limite de tokens atingido')
        else:
            print(f'Erro: {error}')
        
        return None
```
