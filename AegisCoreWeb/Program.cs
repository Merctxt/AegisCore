using AegisCoreWeb.Services;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace AegisCoreWeb;

public class Program
{
    public static void Main(string[] args)
    {
        // Load .env file
        Env.Load("../.env");
        
        var builder = WebApplication.CreateBuilder(args);
        
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
        var apiUrl = Environment.GetEnvironmentVariable("API_URL") ?? "http://localhost:5050";
        builder.Services.AddHttpClient("AegisApi", client =>
        {
            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });
        
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
