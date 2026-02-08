# AegisCore API - Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copia o projeto da API
COPY ["AegisCoreApi/AegisCoreApi.csproj", "AegisCoreApi/"]
RUN dotnet restore "AegisCoreApi/AegisCoreApi.csproj"

# Copia o resto dos arquivos
COPY AegisCoreApi/ AegisCoreApi/
WORKDIR "/src/AegisCoreApi"
RUN dotnet build "AegisCoreApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AegisCoreApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Railway usa a variável PORT
ENV ASPNETCORE_URLS=http://+:${PORT:-80}
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "AegisCoreApi.dll"]
