

using System.Text;
using AegisCoreApi.Data;
using AegisCoreApi.Middleware;
using AegisCoreApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace AegisCoreApi;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        Console.WriteLine($"[CONFIG] Environment: {builder.Environment.EnvironmentName}");
        
        // ========== Configuration ==========
        ConfigureServices(builder);
        
        var app = builder.Build();
        
        // ========== Middleware Pipeline ==========
        ConfigureMiddleware(app);
        
        // ========== Database Migration ==========
        await InitializeDatabaseAsync(app);
        
        await app.RunAsync();
    }
    
    private static void ConfigureServices(WebApplicationBuilder builder)
    {
        var services = builder.Services;
        var configuration = builder.Configuration;
        
        
        // Database - PostgreSQL
        var connectionString = BuildConnectionString(configuration);
        
        services.AddDbContext<AegisDbContext>(options =>
            options.UseNpgsql(connectionString));
        
        // JWT Authentication (prioridade: variável de ambiente > appsettings)
        var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET")
            ?? configuration["Jwt:Secret"] 
            ?? throw new InvalidOperationException("JWT Secret not configured");
        
        Console.WriteLine($"[CONFIG] JWT Secret configured: {!string.IsNullOrEmpty(jwtSecret)}");
        
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
                    ValidateIssuer = true,
                    ValidIssuer = configuration["Jwt:Issuer"] ?? "AegisCore",
                    ValidateAudience = true,
                    ValidAudience = configuration["Jwt:Audience"] ?? "AegisCoreUsers",
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });
        
        services.AddAuthorization();
        
        // Controllers
        services.AddControllers();
        
        // Swagger / OpenAPI
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "AegisCore API",
                Version = "v1",
                Description = "AI-powered content moderation API using Google Perspective API",
                Contact = new OpenApiContact
                {
                    Name = "AegisCore",
                    Url = new Uri("https://github.com/Merctxt/AegisCore")
                }
            });
            
            // JWT Auth
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token.",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });
            
            // API Key Auth
            c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
            {
                Description = "API Key for moderation endpoints. Enter your key (e.g., aegis_xxxxx)",
                Name = "X-Api-Key",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey
            });
            
            // Add both security schemes - endpoints will use one or the other
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                },
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "ApiKey"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });
        
        // HTTP Client Factory
        services.AddHttpClient();
        
        // Services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IApiKeyService, ApiKeyService>();
        services.AddScoped<IPerspectiveService, PerspectiveService>();
        services.AddScoped<IWebhookService, WebhookService>();
        services.AddScoped<IRequestLogService, RequestLogService>();
        services.AddScoped<ApiKeyAuthFilter>();
        
        // CORS
        services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", policy =>
            {
                policy.WithOrigins(
                        configuration["Cors:Origins"]?.Split(',') ?? new[] { "http://localhost:5000", "https://localhost:5001" })
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });
    }
    
    private static void ConfigureMiddleware(WebApplication app)
    {
        // Swagger (always enabled for SaaS)
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "AegisCore API v1");
            c.RoutePrefix = "swagger";
            c.DocumentTitle = "AegisCore API Documentation";
        });
        
        // Não usa HttpsRedirection - Railway já cuida do SSL no proxy
        
        app.UseCors("AllowFrontend");
        
        app.UseAuthentication();
        app.UseAuthorization();
        
        app.MapControllers();
        
        
        // Health check endpoint
        app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));
        
        // Root redirect to swagger
        app.MapGet("/", () => Results.Redirect("/swagger"));
    }
    
    private static async Task InitializeDatabaseAsync(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AegisDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        
        try
        {
            logger.LogInformation("Applying database migrations...");
            await context.Database.MigrateAsync();
            logger.LogInformation("Database migrations applied successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error applying database migrations. Attempting to create database...");
            await context.Database.EnsureCreatedAsync();
        }
    }
    
    /// <summary>
    /// Constrói a connection string do PostgreSQL
    /// Prioridade: 1) DATABASE_URL (Railway/Heroku), 2) Variáveis separadas, 3) appsettings.json
    /// </summary>
    private static string BuildConnectionString(IConfiguration configuration)
    {
        // 1. Tenta DATABASE_URL (formato: postgresql://user:pass@host:port/database)
        var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
        if (!string.IsNullOrEmpty(databaseUrl))
        {
            Console.WriteLine("[DB] Using DATABASE_URL from environment");
            return ConvertPostgresUrl(databaseUrl);
        }
        
        // 2. Tenta variáveis de ambiente separadas
        var envHost = Environment.GetEnvironmentVariable("DB_HOST");
        if (!string.IsNullOrEmpty(envHost))
        {
            Console.WriteLine("[DB] Using DB_* environment variables");
            var envPort = Environment.GetEnvironmentVariable("DB_PORT") ?? "5432";
            var envDatabase = Environment.GetEnvironmentVariable("DB_NAME") ?? "postgres";
            var envUsername = Environment.GetEnvironmentVariable("DB_USERNAME") ?? "postgres";
            var envPassword = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "";
            
            return $"Host={envHost};Port={envPort};Database={envDatabase};Username={envUsername};Password={envPassword};SSL Mode=Prefer;Trust Server Certificate=true";
        }
        
        // 3. Usa appsettings.json
        Console.WriteLine("[DB] Using appsettings.json configuration");
        var host = configuration["Database:Host"] ?? "localhost";
        var port = configuration["Database:Port"] ?? "5432";
        var database = configuration["Database:Name"] ?? "aegiscore";
        var username = configuration["Database:Username"] ?? "postgres";
        var password = configuration["Database:Password"] ?? "postgres";
        
        return $"Host={host};Port={port};Database={database};Username={username};Password={password};SSL Mode=Prefer;Trust Server Certificate=true";
    }
    
    /// <summary>
    /// Converte URL do PostgreSQL para connection string do Npgsql
    /// postgresql://user:pass@host:port/database -> Host=host;Port=port;...
    /// </summary>
    private static string ConvertPostgresUrl(string url)
    {
        try
        {
            var uri = new Uri(url);
            var userInfo = uri.UserInfo.Split(':');
            var username = userInfo.Length > 0 ? userInfo[0] : "postgres";
            var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : "";
            var host = uri.Host;
            var port = uri.Port > 0 ? uri.Port : 5432;
            var database = uri.AbsolutePath.TrimStart('/');
            
            if (string.IsNullOrEmpty(database)) database = "postgres";
            
            return $"Host={host};Port={port};Database={database};Username={username};Password={password};SSL Mode=Prefer;Trust Server Certificate=true";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DB] Error parsing DATABASE_URL: {ex.Message}");
            throw;
        }
    }
}
