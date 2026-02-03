
using System.Text;
using AegisCoreApi.Data;
using AegisCoreApi.Middleware;
using AegisCoreApi.Services;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace AegisCoreApi;

public class Program
{
    public static async Task Main(string[] args)
    {
        // Load .env file
        Env.Load("../.env");
        
        var builder = WebApplication.CreateBuilder(args);
        
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
        var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL") 
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Database connection string not found");
        
        services.AddDbContext<AegisDbContext>(options =>
            options.UseNpgsql(connectionString));
        
        // JWT Authentication
        var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET") 
            ?? configuration["Jwt:Secret"] 
            ?? throw new InvalidOperationException("JWT Secret not configured");
        
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
                Description = "API Key for accessing moderation endpoints",
                Name = "X-Api-Key",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey
            });
            
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
        
        app.UseHttpsRedirection();
        
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
}
