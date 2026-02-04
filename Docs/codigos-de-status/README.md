⬅️ [Voltar para o índice](../README.md)

# 📊 Códigos de Status HTTP

Esta página documenta todos os códigos de status HTTP retornados pela AegisCore API.

---

## Códigos de Sucesso

| Código | Nome | Descrição |
|--------|------|-----------|
| 200 | OK | Requisição processada com sucesso |
| 201 | Created | Recurso criado com sucesso (ex: webhook, API key) |
| 204 | No Content | Requisição bem-sucedida, sem conteúdo de retorno (ex: DELETE) |

---

## Códigos de Erro do Cliente

| Código | Nome | Descrição |
|--------|------|-----------|
| 400 | Bad Request | Dados inválidos na requisição |
| 401 | Unauthorized | API key inválida, ausente ou JWT expirado |
| 403 | Forbidden | Acesso negado ao recurso |
| 404 | Not Found | Endpoint ou recurso não encontrado |
| 422 | Unprocessable Entity | Dados válidos mas não processáveis |
| 429 | Too Many Requests | Rate limit excedido |

---

## Códigos de Erro do Servidor

| Código | Nome | Descrição |
|--------|------|-----------|
| 500 | Internal Server Error | Erro interno do servidor |
| 502 | Bad Gateway | Erro de comunicação com serviço externo (Perspective API) |
| 503 | Service Unavailable | Serviço temporariamente indisponível |

---

## Exemplos de Resposta de Erro

### 400 - Bad Request

```json
{
  "error": "Bad Request",
  "message": "O campo 'text' é obrigatório",
  "details": {
    "field": "text",
    "code": "REQUIRED_FIELD"
  }
}
```

### 401 - Unauthorized

```json
{
  "error": "Unauthorized",
  "message": "API key inválida ou não fornecida"
}
```

### 429 - Rate Limit Excedido

```json
{
  "error": "Too Many Requests",
  "message": "Você atingiu o limite de requisições. Tente novamente em 1 hora.",
  "retryAfter": 3600
}
```

### 500 - Internal Server Error

```json
{
  "error": "Internal Server Error",
  "message": "Ocorreu um erro inesperado. Tente novamente.",
  "requestId": "abc123-def456"
}
```

---

## Tratando Erros

### JavaScript

```javascript
try {
  const response = await fetch('/api/moderation/analyze', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'X-Api-Key': 'sua_chave'
    },
    body: JSON.stringify({ text: 'Texto' })
  });

  if (!response.ok) {
    const error = await response.json();
    
    switch (response.status) {
      case 400:
        console.error('Dados inválidos:', error.message);
        break;
      case 401:
        console.error('Autenticação falhou:', error.message);
        break;
      case 429:
        console.error('Rate limit. Aguarde:', error.retryAfter, 'segundos');
        break;
      default:
        console.error('Erro:', error.message);
    }
    return;
  }

  const data = await response.json();
  console.log('Sucesso:', data);
} catch (error) {
  console.error('Erro de rede:', error);
}
```

### Python

```python
import requests
from requests.exceptions import HTTPError

try:
    response = requests.post(
        'https://api.aegiscore.com/api/moderation/analyze',
        json={'text': 'Texto'},
        headers={'X-Api-Key': 'sua_chave'}
    )
    response.raise_for_status()
    data = response.json()
    print('Sucesso:', data)

except HTTPError as e:
    error = e.response.json()
    
    if e.response.status_code == 429:
        retry_after = error.get('retryAfter', 60)
        print(f'Rate limit. Aguarde {retry_after} segundos')
    else:
        print(f'Erro {e.response.status_code}: {error.get("message")}')
```