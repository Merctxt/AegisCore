using System.Diagnostics;
using AegisCoreWeb.Models;
using AegisCoreWeb.Services;
using Microsoft.AspNetCore.Mvc;

namespace AegisCoreWeb.Controllers;

public class HomeController : Controller
{
    private readonly IApiService _apiService;
    private readonly ILogger<HomeController> _logger;
    
    public HomeController(IApiService apiService, ILogger<HomeController> logger)
    {
        _apiService = apiService;
        _logger = logger;
    }
    
    public async Task<IActionResult> Index()
    {
        // Get plans for landing page
        var plans = await _apiService.GetAsync<List<PlanViewModel>>("api/plans");
        ViewBag.Plans = plans ?? new List<PlanViewModel>();
        
        return View();
    }
    
    public IActionResult Pricing()
    {
        return View();
    }
    
    public IActionResult Documentation()
    {
        return View();
    }
    
    public IActionResult Privacy()
    {
        return View();
    }
    
    public IActionResult Terms()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
