# AegisCore - Documentação Técnica

API REST para moderação automática de conteúdo usando Google Perspective API. Este documento é destinado a desenvolvedores que desejam hospedar sua própria instância.

## Índice

1. [Instalação e Configuração](./instalacao-configuracao/README.md)
2. [Autenticação](./autenticacao/README.md)
3. [Rate Limiting](./rate-limiting/README.md)
4. [Endpoints](./endpoints/README.md)
5. [Exemplos de Uso](./exemplos-de-uso/README.md)
6. [Códigos de Status](./codigos-de-status/README.md)
7. [Integração com Aplicações](./integracao-com-aplicacoes/README.md)

## Arquitetura

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
│   ├── Models/            # ViewModels
│   └── Services/          # Serviços HTTP
└── Docs/                  # Documentação
```

## Início Rápido

Para começar a usar a AegisCore API, siga os passos:

1. **[Instale e configure](./instalacao-configuracao/README.md)** o ambiente
2. **[Configure a autenticação](./autenticacao/README.md)** (JWT ou API Key)
3. **[Consulte os endpoints](./endpoints/README.md)** disponíveis
4. **[Veja exemplos práticos](./exemplos-de-uso/README.md)** em várias linguagens
