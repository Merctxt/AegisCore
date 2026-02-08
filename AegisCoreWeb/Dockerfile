# AegisCore Web - Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copia o projeto Web
COPY ["AegisCoreWeb/AegisCoreWeb.csproj", "AegisCoreWeb/"]
RUN dotnet restore "AegisCoreWeb/AegisCoreWeb.csproj"

# Copia o resto dos arquivos
COPY AegisCoreWeb/ AegisCoreWeb/
WORKDIR "/src/AegisCoreWeb"
RUN dotnet build "AegisCoreWeb.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AegisCoreWeb.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Railway usa a variável PORT
ENV ASPNETCORE_URLS=http://+:${PORT:-80}
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "AegisCoreWeb.dll"]
