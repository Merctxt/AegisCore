⬅️ [Voltar para o índice](../README.md)

# 📡 Endpoints

Documentação completa de todos os endpoints disponíveis na AegisCore API.

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
  "timestamp": "2026-01-15T10:30:00Z"
}
```

---

## Análise de Texto

Analisa um texto em busca de conteúdo tóxico.

```http
POST /api/moderation/analyze
X-Api-Key: sua_chave
Content-Type: application/json
```

**Body:**
```json
{
  "text": "Texto para analisar",
  "language": "pt",
  "includeAllScores": false
}
```

**Resposta:**
```json
{
  "isToxic": false,
  "toxicityScore": 0.12,
  "allScores": null,
  "analyzedText": "Texto para analisar",
  "timestamp": "2026-01-15T10:30:00Z"
}
```

### Parâmetros

| Campo | Tipo | Obrigatório | Descrição |
|-------|------|-------------|-----------|
| `text` | string | ✅ | Texto para analisar (máx. 3000 caracteres) |
| `language` | string | ❌ | Idioma do texto (`pt`, `en`, `es`, etc.) |
| `includeAllScores` | boolean | ❌ | Incluir todas as pontuações de análise |

### Resposta com `includeAllScores: true`

```json
{
  "isToxic": false,
  "toxicityScore": 0.12,
  "allScores": {
    "toxicity": 0.12,
    "severeToxicity": 0.05,
    "identityAttack": 0.08,
    "insult": 0.10,
    "profanity": 0.03,
    "threat": 0.02
  },
  "analyzedText": "Texto para analisar",
  "timestamp": "2026-01-15T10:30:00Z"
}
```

---

## Análise em Lote

Analisa múltiplos textos em uma única requisição.

```http
POST /api/moderation/analyze/batch
X-Api-Key: sua_chave
Content-Type: application/json
```

**Body:**
```json
{
  "texts": ["texto1", "texto2", "texto3"],
  "language": "pt"
}
```

**Resposta:**
```json
{
  "results": [
    {
      "index": 0,
      "text": "texto1",
      "isToxic": false,
      "toxicityScore": 0.10
    },
    {
      "index": 1,
      "text": "texto2",
      "isToxic": false,
      "toxicityScore": 0.08
    },
    {
      "index": 2,
      "text": "texto3",
      "isToxic": true,
      "toxicityScore": 0.85
    }
  ],
  "summary": {
    "total": 3,
    "toxic": 1,
    "safe": 2
  },
  "timestamp": "2026-01-15T10:30:00Z"
}
```

---

## Gerenciamento de API Keys

### Listar API Keys

```http
GET /api/apikeys
Authorization: Bearer seu_jwt_token
```

### Criar API Key

```http
POST /api/apikeys
Authorization: Bearer seu_jwt_token
Content-Type: application/json

{
  "name": "Minha Aplicação"
}
```

### Revogar API Key

```http
DELETE /api/apikeys/{id}
Authorization: Bearer seu_jwt_token
```

---

## Gerenciamento de Webhooks

### Listar Webhooks

```http
GET /api/webhooks
Authorization: Bearer seu_jwt_token
```

### Criar Webhook

```http
POST /api/webhooks
Authorization: Bearer seu_jwt_token
Content-Type: application/json

{
  "name": "Alertas Toxicidade",
  "url": "https://seu-servidor.com/webhook",
  "secret": "chave_para_validacao",
  "events": 1
}
```

### Atualizar Webhook

```http
PUT /api/webhooks/{id}
Authorization: Bearer seu_jwt_token
Content-Type: application/json

{
  "name": "Novo Nome",
  "url": "https://novo-servidor.com/webhook",
  "events": 7
}
```

### Deletar Webhook

```http
DELETE /api/webhooks/{id}
Authorization: Bearer seu_jwt_token
```

---

## ⚙️ Thresholds Personalizados

Configure thresholds para cada tipo de análise:

```json
{
  "text": "Texto para analisar",
  "thresholds": {
    "toxicity": 0.8,        // Toxicidade geral
    "severeToxicity": 0.9,  // Toxicidade severa
    "identityAttack": 0.7,  // Ataques de identidade
    "insult": 0.6,          // Insultos
    "profanity": 0.5,       // Palavrões
    "threat": 0.8           // Ameaças
  }
}
```