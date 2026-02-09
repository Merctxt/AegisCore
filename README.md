# AegisCore - API de Moderação de Conteúdo com IA

Sistema completo de moderação automática de conteúdo utilizando Google Perspective API, construído com ASP.NET Core.

## Funcionalidades

### API REST (AegisCoreApi)
- **Análise de toxicidade** em tempo real via Google Perspective API
- **Análise em lote** para processar múltiplos textos
- **Autenticação dupla**: JWT para dashboard e API Keys para integrações
- **Rate limiting** por plano (Free, Starter, Pro, Enterprise)
- **Webhooks** para notificações de conteúdo tóxico
- **Logs de requisições** para auditoria


## Pré-requisitos

- .NET 9 SDK
- PostgreSQL 14+
- Google Perspective API Key

## Documentação

Documentação completa disponível em [Docs/README.md](Docs/README.md):

- [Instalação e Configuração](Docs/instalacao-configuracao/README.md)
- [Autenticação](Docs/autenticacao/README.md)
- [Rate Limiting](Docs/rate-limiting/README.md)
- [Endpoints](Docs/endpoints/README.md)
- [Exemplos de Uso](Docs/exemplos-de-uso/README.md)
- [Códigos de Status](Docs/codigos-de-status/README.md)
- [Integração com Aplicações](Docs/integracao-com-aplicacoes/README.md)

## Tipos de Conteúdo Detectados

- Toxicidade geral
- Toxicidade severa
- Ataques de identidade
- Insultos
- Profanidade
- Ameaças

## Como Obter a Google Perspective API Key

1. Acesse [Google Cloud Console](https://console.cloud.google.com/)
2. Crie um novo projeto ou selecione um existente
3. Ative a **Perspective Comment Analyzer API**
4. Crie uma credencial de API Key
5. Copie a chave para o `.env`

