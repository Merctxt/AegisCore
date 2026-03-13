# AegisCore API - Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80

ENV LANG=C.UTF-8
ENV LC_ALL=C.UTF-8
ENV ASPNETCORE_ENVIRONMENT=Production

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copia o csproj
COPY ["AegisCoreApi.csproj", "./"]
RUN dotnet restore "AegisCoreApi.csproj"

# Copia o resto
COPY . .
RUN dotnet build "AegisCoreApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AegisCoreApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "AegisCoreApi.dll"]
