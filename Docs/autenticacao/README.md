 [Voltar para o índice](../README.md)

# Autenticação

A AegisCore API usa um sistema de **Token de Acesso** simples e seguro.

---

## Como Funciona

1. **Gere um token** via `POST /api/token/generate`
2. **Use o token** no header `X-Access-Token` das requisições
3. **Token expira** após 30 minutos (gere outro quando necessário)

---

## Gerar Token

```http
POST /api/token/generate
```

**Resposta:**
```json
{
  "token": "aegis_abc123xyz789...",
  "expiresAt": "2026-02-21T23:30:00Z",
  "expiresInMinutes": 30,
  "usage": "Inclua o token no header 'X-Access-Token' para usar a API de moderação"
}
```

---

## Usar o Token

Inclua o token no header `X-Access-Token`:

```http
POST /api/moderation/analyze
X-Access-Token: aegis_abc123xyz789...
Content-Type: application/json

{
  "text": "Texto para analisar"
}
```

---

## Verificar Status do Token

```http
GET /api/token/status
X-Access-Token: aegis_abc123xyz789...
```

**Resposta:**
```json
{
  "isActive": true,
  "expiresAt": "2026-02-21T23:30:00Z",
  "remainingMinutes": 25.5,
  "requestCount": 10,
  "createdAt": "2026-02-21T23:00:00Z"
}
```

---

## Limites

| Limite | Valor |
|--------|-------|
| Tokens ativos por IP | 2 |
| Duração do token | 30 minutos |

Quando você já tiver 2 tokens ativos, receberá erro `429`:

```json
{
  "error": "Limite de tokens atingido",
  "message": "Você já possui 2 tokens ativos. Aguarde a expiração ou use um token existente.",
  "limit": 2
}
```

---

## Boas Práticas

1. **Guarde seu token** após gerá-lo
2. **Reutilize o token** enquanto estiver válido
3. **Verifique o status** antes de gerar um novo
4. **Use HTTPS** sempre em produção