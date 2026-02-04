⬅️ [Voltar para o índice](../README.md)

# ⚡ Rate Limiting

A API implementa rate limiting para garantir estabilidade e uso justo dos recursos.

---

## Limites por Plano

| Plano      | Requisições/dia | Requisições/minuto |
|------------|-----------------|-------------------|
| Free       | 100             | 10                |
| Starter    | 1,000           | 30                |
| Pro        | 10,000          | 100               |
| Enterprise | Ilimitado       | Ilimitado         |

---

## Headers de Resposta

A API retorna headers informativos sobre seu uso:

```http
X-RateLimit-Limit: 100
X-RateLimit-Remaining: 95
X-RateLimit-Reset: 1706918400
```

| Header | Descrição |
|--------|-----------|
| `X-RateLimit-Limit` | Limite total de requisições |
| `X-RateLimit-Remaining` | Requisições restantes |
| `X-RateLimit-Reset` | Timestamp Unix de quando o limite reseta |

---

## Resposta de Rate Limit Excedido

Quando o limite é atingido, a API retorna:

```http
HTTP/1.1 429 Too Many Requests
Content-Type: application/json
Retry-After: 3600

{
  "error": "Rate limit exceeded",
  "message": "Você atingiu o limite de requisições. Tente novamente em 1 hora.",
  "retryAfter": 3600
}
```

---

## ⚙️ Configuração Personalizada

Para ajustar os limites (self-hosted), edite a configuração:

```json
// appsettings.json
{
  "RateLimiting": {
    "EnableRateLimiting": true,
    "PermitLimit": 100,
    "Window": "00:01:00",
    "QueueLimit": 2
  }
}
```

---

## Boas Práticas

1. **Implemente retry com backoff exponencial**
2. **Cache resultados** quando possível
3. **Use análise em lote** para múltiplos textos
4. **Monitore headers** de rate limit
5. **Considere upgrade de plano** para maior volume

### Exemplo de Retry com Backoff

```javascript
async function analyzeWithRetry(text, maxRetries = 3) {
  for (let i = 0; i < maxRetries; i++) {
    try {
      const response = await fetch('/api/moderation/analyze', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'X-Api-Key': 'sua_chave'
        },
        body: JSON.stringify({ text })
      });

      if (response.status === 429) {
        const retryAfter = response.headers.get('Retry-After') || Math.pow(2, i);
        await new Promise(r => setTimeout(r, retryAfter * 1000));
        continue;
      }

      return await response.json();
    } catch (error) {
      if (i === maxRetries - 1) throw error;
      await new Promise(r => setTimeout(r, Math.pow(2, i) * 1000));
    }
  }
}
```