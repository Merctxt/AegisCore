 [Voltar para o índice](../README.md)

# Instalação e Configuração

## Pré-requisitos

- .NET 9 SDK
- PostgreSQL 14+
- Google Perspective API Key

## 1. Clone o repositório

```bash
git clone https://github.com/Merctxt/AegisCore.git
cd AegisCore
```

## 2. Configure as variáveis de ambiente

Crie um arquivo `.env` ou configure as variáveis:

```env
# Database (PostgreSQL)
DATABASE_URL=postgresql://user:password@localhost:5432/aegiscore

# Ou variáveis separadas:
DB_HOST=localhost
DB_PORT=5432
DB_NAME=aegiscore
DB_USERNAME=postgres
DB_PASSWORD=sua_senha

# Google Perspective API
PERSPECTIVE_API_KEY=sua_chave_aqui
```

## 3. Execute as migrations

```bash
cd AegisCoreApi
dotnet ef database update
```

## 4. Inicie a API

```bash
dotnet run
```

A API estará disponível em `http://localhost:5050`

---

## Docker

```bash
docker-compose up -d
```

O `docker-compose.yml` já está configurado na raiz do projeto.

---

## Deploy em Produção

### Variáveis de Ambiente

```env
ASPNETCORE_ENVIRONMENT=Production
DATABASE_URL=postgresql://user:password@host:5432/aegiscore_prod
PERSPECTIVE_API_KEY=sua_chave_production
```

### Dockerfile

O Dockerfile está na pasta `AegisCoreApi/`:

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["AegisCoreApi.csproj", "./"]
RUN dotnet restore
COPY . .
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AegisCoreApi.dll"]
```

---

## Railway / Render / Fly.io

A API está pronta para deploy em plataformas cloud:

1. Configure `DATABASE_URL` e `PERSPECTIVE_API_KEY`
2. A API detecta automaticamente a porta via variável `PORT`
3. Migrations são aplicadas automaticamente no startup

---

## Proxy Reverso (Nginx)

```nginx
server {
    listen 80;
    server_name api.exemplo.com;
    
    location / {
        proxy_pass http://localhost:5050;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

---

## Health Check

Configure monitoramento:

```bash
curl -f http://localhost:5050/health || exit 1
```

Resposta esperada:
```json
{
  "status": "healthy",
  "timestamp": "2026-02-21T22:00:00Z"
}
```
