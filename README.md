# AegisCore - AI Content Moderation API

![.NET](https://img.shields.io/badge/.NET-9-blue)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-14+-blue)
![License](https://img.shields.io/badge/License-CC0%201.0-lightgrey)
![Status](https://img.shields.io/badge/Status-Production--Ready-brightgreen)



AegisCore is a scalable content moderation API built with ASP.NET Core, powered by Google Perspective API.

It provides real-time toxicity detection, plan-based rate limiting, API key authentication, and full request logging — designed for platforms that need reliable automated moderation.

**Live API Documentation (Swagger):**  
https://api.giovannidev.com/swagger

## Documentation

Comprehensive documentation is available in the `Docs/` directory:

- [Installation & Configuration](Docs/instalacao-configuracao/README.md)
- [Authentication](Docs/autenticacao/README.md)
- [Rate Limiting](Docs/rate-limiting/README.md)
- [Endpoints](Docs/endpoints/README.md)
- [examples of use](Docs/exemplos-de-uso/README.md)
- [status codes](Docs/codigos-de-status/README.md)
- [Integration with Applications](Docs/integracao-com-aplicacoes/README.md)



## Features

- Real-time toxicity analysis
- Batch text analysis
- JWT authentication
- API Key authentication (integrations)
- Plan-based rate limiting (Free, Starter, Pro, Enterprise)
- Request logging and usage tracking

### Detected Content Types

- Toxicity
- Severe Toxicity
- Identity Attacks
- Insults
- Profanity
- Threats



## Tech Stack

- **Backend:** ASP.NET Core (.NET 9)
- **Database:** PostgreSQL
- **Authentication:** JWT + API Keys
- **AI Provider:** Google Perspective API
- **API Documentation:** Swagger / OpenAPI



## Getting Started

### Prerequisites

- .NET 9 SDK
- PostgreSQL 14+
- Google Perspective API Key

