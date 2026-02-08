using AegisCoreWeb.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace AegisCoreWeb;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
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
        
        // HTTP Client for API
        builder.Services.AddHttpClient("AegisApi", client =>
        {
            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });
        
        // Registra a URL da API para uso nas Views
        builder.Services.AddSingleton(new AppSettings { ApiUrl = apiUrl });
        
        // Services
        builder.Services.AddScoped<IApiService, ApiService>();
        
        var app = builder.Build();


        // Configure the HTTP request pipeline
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
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
