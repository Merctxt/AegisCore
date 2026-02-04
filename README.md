# 🛡️ AegisCore - API de Moderação de Conteúdo com IA

Sistema completo de moderação automática de conteúdo utilizando Google Perspective API, construído com ASP.NET Core.

## 🚀 Funcionalidades

### API REST (AegisCoreApi)
- **Análise de toxicidade** em tempo real via Google Perspective API
- **Análise em lote** para processar múltiplos textos
- **Autenticação dupla**: JWT para dashboard e API Keys para integrações
- **Rate limiting** por plano (Free, Starter, Pro, Enterprise)
- **Webhooks** para notificações de conteúdo tóxico
- **Logs de requisições** para auditoria

### Dashboard Web (AegisCoreWeb)
- **Interface web** para gerenciamento
- **Gerenciamento de API Keys**
- **Visualização de estatísticas**
- **Configuração de webhooks**

## 📋 Pré-requisitos

- .NET 10 SDK
- PostgreSQL 14+
- Google Perspective API Key

## 🛠️ Instalação Rápida

1. **Clone o repositório:**
```bash
git clone https://github.com/Merctxt/AegisCore.git
cd AegisCore
```

2. **Configure as variáveis de ambiente:**
```bash
cp .env.example .env
```

Edite o arquivo `.env`:
```env
DATABASE_URL=postgresql://user:password@localhost:5432/aegiscore
JWT_SECRET=sua-chave-secreta-com-pelo-menos-32-caracteres!
PERSPECTIVE_API_KEY=sua_chave_aqui
```

3. **Execute as migrations:**
```bash
cd AegisCoreApi
dotnet ef migrations add InitialCreate
dotnet ef database update
```

4. **Inicie a API:**
```bash
dotnet run --project AegisCoreApi
```

A API estará disponível em `https://localhost:5050`

5. **Inicie o Dashboard (opcional):**
```bash
dotnet run --project AegisCoreWeb
```

## 📡 Uso da API

### Análise de Texto

```http
POST /api/moderation/analyze
X-Api-Key: aegis_sua_chave
Content-Type: application/json

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
  "analyzedText": "Texto para analisar",
  "timestamp": "2026-01-15T10:30:00Z"
}
```

### Análise em Lote

```http
POST /api/moderation/analyze/batch
X-Api-Key: sua_chave

{
  "texts": ["texto1", "texto2", "texto3"],
  "language": "pt"
}
```

## ⚡ Rate Limiting

| Plano      | Requisições/dia |
|------------|-----------------|
| Free       | 100             |
| Starter    | 1,000           |
| Pro        | 10,000          |
| Enterprise | Ilimitado       |

## 🏗️ Arquitetura

```
AegisCore/
├── AegisCoreApi/          # Backend API (ASP.NET Core)
│   ├── Controllers/       # Endpoints da API
│   ├── Services/          # Lógica de negócio
│   ├── Models/            # Entidades do banco
│   ├── DTOs/              # Data Transfer Objects
│   ├── Data/              # DbContext (PostgreSQL)
│   └── Middleware/        # Autenticação API Key
├── AegisCoreWeb/          # Frontend (ASP.NET MVC)
│   ├── Controllers/       # Controllers MVC
│   ├── Views/             # Razor Views
│   └── Services/          # Serviços HTTP
└── Docs/                  # Documentação técnica
```

## 📚 Documentação

Documentação completa disponível em [Docs/README.md](Docs/README.md):

- [Instalação e Configuração](Docs/instalacao-configuracao/README.md)
- [Autenticação](Docs/autenticacao/README.md)
- [Rate Limiting](Docs/rate-limiting/README.md)
- [Endpoints](Docs/endpoints/README.md)
- [Exemplos de Uso](Docs/exemplos-de-uso/README.md)
- [Códigos de Status](Docs/codigos-de-status/README.md)
- [Integração com Aplicações](Docs/integracao-com-aplicacoes/README.md)

## 🔍 Tipos de Conteúdo Detectados

- Toxicidade geral
- Toxicidade severa
- Ataques de identidade
- Insultos
- Profanidade
- Ameaças

## 🔧 Como Obter a Google Perspective API Key

1. Acesse [Google Cloud Console](https://console.cloud.google.com/)
2. Crie um novo projeto ou selecione um existente
3. Ative a **Perspective Comment Analyzer API**
4. Crie uma credencial de API Key
5. Copie a chave para o `.env`

## 🐛 Solução de Problemas

### API não responde
- Verifique se a conexão com PostgreSQL está correta
- Verifique se as migrations foram aplicadas
- Verifique os logs do console

### Erros de autenticação
- Confirme que a API Key está no header `X-Api-Key`
- Verifique se o JWT não expirou

### Rate limit excedido
- Aguarde o reset do limite (verificar header `Retry-After`)
- Considere upgrade de plano para maior volume

## 📄 Licença

MIT License - Veja [LICENSE](LICENSE) para mais detalhes.

---

**Versão:** 1.0.0  
**Última atualização:** Fevereiro 2026

