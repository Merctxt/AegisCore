 [Voltar para o índice](../README.md)

# Autenticação

A AegisCore API suporta dois métodos de autenticação: **JWT Token** e **API Key**.

---

## JWT Token (para Dashboard)

Use JWT para acessar endpoints do dashboard e gerenciar sua conta.

### Login

```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "usuario@email.com",
  "password": "suasenha"
}
```

### Resposta

```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2026-01-22T12:00:00Z",
  "user": {
    "id": "guid",
    "name": "Nome",
    "email": "email@email.com",
    "plan": "Free"
  }
}
```

### Uso do Token

Inclua o token no header `Authorization`:

```http
GET /api/user/profile
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

---

## API Key (para Moderation Endpoints)

Use API Keys para acessar os endpoints de moderação de conteúdo.

### Obter API Key

1. Faça login no dashboard
2. Navegue até "API Keys"
3. Clique em "Gerar Nova Chave"

### Uso da API Key

Inclua a chave no header `X-Api-Key`:

```http
POST /api/moderation/analyze
X-Api-Key: aegis_sua_chave_aqui
Content-Type: application/json

{
  "text": "Texto para analisar"
}
```

---

## Webhooks

Configure webhooks para receber notificações quando conteúdo tóxico for detectado.

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

### Eventos Disponíveis

| Valor | Evento |
|-------|--------|
| `1` | Conteúdo Tóxico |
| `2` | Alta Toxicidade (>90%) |
| `4` | Rate Limit Atingido |
| `7` | Todos os Eventos |

### Payload do Webhook

```json
{
  "event": "ToxicContent",
  "timestamp": "2026-01-15T10:30:00Z",
  "data": {
    "text": "texto analisado",
    "toxicityScore": 0.85,
    "analyzedAt": "2026-01-15T10:30:00Z"
  }
}
```

### Validação de Assinatura

Se você configurou um `secret`, valide a assinatura no header:

```
X-Aegis-Signature: sha256=hash_hmac_do_payload
```

---

## Boas Práticas de Segurança

1. **Nunca exponha** sua API Key no frontend
2. **Rotacione** suas API Keys periodicamente
3. **Use variáveis de ambiente** para armazenar secrets
4. **Valide assinaturas** dos webhooks recebidos
5. **Use HTTPS** sempre em produção