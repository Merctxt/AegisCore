 [Voltar para o índice](../README.md)

# Endpoints

Documentação completa de todos os endpoints disponíveis na AegisCore API.

---

## Token

### Gerar Token

Cria um novo token de acesso para usar a API.

```http
POST /api/token/generate
```

**Resposta (200):**
```json
{
  "token": "aegis_abc123xyz789...",
  "expiresAt": "2026-02-21T23:30:00Z",
  "expiresInMinutes": 30,
  "usage": "Inclua o token no header 'X-Access-Token' para usar a API de moderação"
}
```

**Erro (429) - Limite atingido:**
```json
{
  "error": "Limite de tokens atingido",
  "message": "Você já possui 2 tokens ativos. Aguarde a expiração ou use um token existente.",
  "limit": 2
}
```

---

### Verificar Status do Token

```http
GET /api/token/status
X-Access-Token: aegis_seu_token
```

**Resposta (200):**
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

## Health Check

Verifica se a API está funcionando.

```http
GET /health
```

**Resposta:**
```json
{
  "status": "healthy",
  "timestamp": "2026-02-21T22:00:00Z"
}
```

---

## Análise de Texto

Analisa um texto em busca de conteúdo tóxico.

```http
POST /api/moderation/analyze
X-Access-Token: aegis_seu_token
Content-Type: application/json
```

**Body:**
```json
{
  "text": "Texto para analisar",
  "language": "pt",
  "includeAllScores": false,
  "toxicityThreshold": 0.7
}
```

**Resposta:**
```json
{
  "isToxic": false,
  "toxicityScore": 0.12,
  "thresholdUsed": 0.7,
  "allScores": null,
  "analyzedText": "Texto para analisar",
  "timestamp": "2026-02-21T22:00:00Z"
}
```

### Parâmetros

| Campo | Tipo | Obrigatório | Descrição |
|-------|------|-------------|-----------|
| `text` | string | ✅ | Texto para analisar (máx. 3000 caracteres) |
| `language` | string | ❌ | Idioma do texto (`pt`, `en`, `es`, etc.). Padrão: `pt` |
| `includeAllScores` | boolean | ❌ | Incluir todas as pontuações. Padrão: `false` |
| `toxicityThreshold` | number | ❌ | Threshold para considerar tóxico (0.0-1.0). Padrão: `0.7` |

### Resposta com `includeAllScores: true`

```json
{
  "isToxic": false,
  "toxicityScore": 0.12,
  "thresholdUsed": 0.7,
  "allScores": {
    "toxicity": 0.12,
    "severeToxicity": 0.05,
    "identityAttack": 0.08,
    "insult": 0.10,
    "profanity": 0.03,
    "threat": 0.02
  },
  "analyzedText": "Texto para analisar",
  "timestamp": "2026-02-21T22:00:00Z"
}
```

---

## Análise em Lote

Analisa múltiplos textos em uma única requisição.

```http
POST /api/moderation/analyze/batch
X-Access-Token: aegis_seu_token
Content-Type: application/json
```

**Body:**
```json
{
  "texts": ["texto1", "texto2", "texto3"],
  "language": "pt",
  "toxicityThreshold": 0.7
}
```

**Resposta:**
```json
{
  "results": [
    {
      "text": "texto1",
      "isToxic": false,
      "toxicityScore": 0.10
    },
    {
      "text": "texto2",
      "isToxic": false,
      "toxicityScore": 0.08
    },
    {
      "text": "texto3",
      "isToxic": true,
      "toxicityScore": 0.85
    }
  ],
  "totalAnalyzed": 3,
  "toxicCount": 1,
  "timestamp": "2026-02-21T22:00:00Z"
}
```

### Limites

- Máximo de **100 textos** por requisição
