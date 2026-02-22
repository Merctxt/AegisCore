# AegisCore - Documentação Técnica

API REST para moderação automática de conteúdo usando Google Perspective API. 

## Índice

1. [Instalação e Configuração](./instalacao-configuracao/README.md)
2. [Autenticação](./autenticacao/README.md)
3. [Endpoints](./endpoints/README.md)
4. [Exemplos de Uso](./exemplos-de-uso/README.md)
5. [Códigos de Status](./codigos-de-status/README.md)
6. [Integração com Aplicações](./integracao-com-aplicacoes/README.md)

## Arquitetura

```
AegisCore/
├── AegisCoreApi/          # Backend API (ASP.NET Core)
│   ├── Controllers/       # Endpoints da API
│   │   ├── TokenController.cs      # Geração de tokens
│   │   └── ModerationController.cs # Análise de conteúdo
│   ├── Services/          # Lógica de negócio
│   ├── Models/            # Entidades do banco
│   ├── DTOs/              # Data Transfer Objects
│   ├── Data/              # DbContext (PostgreSQL)
│   └── Middleware/        # Autenticação por Token
└── Docs/                  # Documentação
```

## Workflow Simplificado

```
1. Gerar Token  →  POST /api/token/generate
                   (válido por 30 min, máx 2 por IP)

2. Usar API     →  POST /api/moderation/analyze
                   Header: X-Access-Token: aegis_xxx
```

## Início Rápido

```bash
# 1. Gerar um token de acesso
curl -X POST https://api.exemplo.com/api/token/generate

# Resposta:
# {
#   "token": "aegis_abc123...",
#   "expiresAt": "2026-02-21T23:00:00Z",
#   "expiresInMinutes": 30
# }

# 2. Analisar texto
curl -X POST https://api.exemplo.com/api/moderation/analyze \
  -H "X-Access-Token: aegis_abc123..." \
  -H "Content-Type: application/json" \
  -d '{"text": "Texto para analisar"}'
```
