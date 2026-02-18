 [Voltar para o índice](../README.md)

# Instalação e Configuração

## Pré-requisitos

- .NET 10 SDK
- PostgreSQL 14+
- Google Perspective API Key

## 1. Clone o repositório

```bash
git clone https://github.com/Merctxt/AegisCore.git
cd AegisCore
```

## 2. Configure as variáveis de ambiente

```bash
cp .env.example .env
```

Edite o arquivo `.env`:

```env
# Database (PostgreSQL)
DATABASE_URL=postgresql://user:password@localhost:5432/aegiscore

# JWT Authentication
JWT_SECRET=sua-chave-secreta-com-pelo-menos-32-caracteres!

# Google Perspective API
PERSPECTIVE_API_KEY=sua_chave_aqui
```

## 3. Execute as migrations

```bash
cd AegisCoreApi
dotnet ef migrations add InitialCreate
dotnet ef database update
```

## 4. Inicie a API

```bash
dotnet run --project AegisCoreApi
```

A API estará disponível em `https://localhost:5050`

---

## Docker (Em breve)

```dockerfile
# Dockerfile disponível em breve
docker-compose up -d
```

---

## Deploy em Produção

### Variáveis de Ambiente para Produção

```env
ASPNETCORE_ENVIRONMENT=Production
DATABASE_URL=postgresql://user:password@host:5432/aegiscore_prod
JWT_SECRET=chave-super-secreta-production
PERSPECTIVE_API_KEY=sua_chave_production
```

### Docker

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["AegisCoreApi/AegisCoreApi.csproj", "AegisCoreApi/"]
RUN dotnet restore "AegisCoreApi/AegisCoreApi.csproj"
COPY . .
WORKDIR "/src/AegisCoreApi"
RUN dotnet build "AegisCoreApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AegisCoreApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AegisCoreApi.dll"]
```

---

## Segurança

### Boas Práticas

1. **Mude a API key padrão** no `.env`
2. **Use HTTPS** em produção
3. **Implemente logs de auditoria**
4. **Configure firewall** para limitar acesso
5. **Monitore uso da API** regularmente

### Exemplo de Proxy Reverso (Nginx)

```nginx
server {
    listen 80;
    server_name sua-api.exemplo.com;
    
    location / {
        proxy_pass http://localhost:5050;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        
        # Rate limiting adicional
        limit_req zone=api_limit burst=20 nodelay;
    }
}
```

---

## Monitoramento

### Health Check

Configure monitoramento automático:

```bash
# Verificar se a API está funcionando
curl -f https://localhost:5050/health || exit 1
```

### Logs

Os logs incluem:
- Todas as requisições com request ID
- Erros da Perspective API
- Rate limiting ativado
- Estatísticas de uso
