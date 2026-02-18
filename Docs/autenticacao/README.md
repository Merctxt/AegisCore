 [Voltar para o índice](../README.md)

# Autenticação

A AegisCore API suporta dois métodos de autenticação: **JWT Token** e **API Key**.

---

## JWT Token (para Administração)

Use JWT para acessar endpoints administrativos e gerenciar sua conta.

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

1. Faça login via `/api/auth/login`
2. Use o endpoint `POST /api/apikeys` para criar uma nova chave

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

## Boas Práticas de Segurança

1. **Nunca exponha** sua API Key no frontend
2. **Rotacione** suas API Keys periodicamente
3. **Use variáveis de ambiente** para armazenar secrets
4. **Use HTTPS** sempre em produção