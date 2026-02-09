using AegisCoreWeb.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;

namespace AegisCoreWeb;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        // Configura para usar a porta do Railway
        var port = Environment.GetEnvironmentVariable("PORT") ?? "5100";
        builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
        Console.WriteLine($"[CONFIG] Listening on port: {port}");
        
        // Determina a URL da API (prioridade: variável de ambiente > appsettings)
        var apiUrl = Environment.GetEnvironmentVariable("API_URL")
            ?? builder.Configuration["Urls:Api"] 
            ?? "http://localhost:5050";
        
        Console.WriteLine($"[CONFIG] Environment: {builder.Environment.EnvironmentName}");
        Console.WriteLine($"[CONFIG] API URL: {apiUrl}");
        
        // Add services to the container
        builder.Services.AddControllersWithViews();
        
        
        // Session
        builder.Services.AddDistributedMemoryCache();
        builder.Services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromHours(24);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
        });
        
        // Cookie Authentication
        builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.LogoutPath = "/Account/Logout";
                options.ExpireTimeSpan = TimeSpan.FromDays(7);
                options.SlidingExpiration = true;
            });
        
        // HTTP Client for API - configurado para aceitar certificados em producao
        builder.Services.AddHttpClient("AegisApi", client =>
        {
            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        })
        .ConfigurePrimaryHttpMessageHandler(() =>
        {
            var handler = new HttpClientHandler();
            // Em producao, aceita certificados da Railway
            if (!builder.Environment.IsDevelopment())
            {
                handler.ServerCertificateCustomValidationCallback = 
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
            }
            return handler;
        });
        
        // Registra a URL da API para uso nas Views
        builder.Services.AddSingleton(new AppSettings { ApiUrl = apiUrl });
        
        // Services
        builder.Services.AddScoped<IApiService, ApiService>();
        
        var app = builder.Build();


        // IMPORTANTE: ForwardedHeaders PRIMEIRO - necessario para Railway/proxies
        app.UseForwardedHeaders(new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
        });

        // Configure the HTTP request pipeline
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            // NAO usa HSTS nem HttpsRedirection - Railway ja cuida do SSL
        }

        app.UseRouting();
        
        app.UseSession();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapStaticAssets();
        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}")
            .WithStaticAssets();


        app.Run();
    }
}
