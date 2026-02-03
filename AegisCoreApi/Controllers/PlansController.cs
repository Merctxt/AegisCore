using AegisCoreApi.DTOs;
using AegisCoreApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace AegisCoreApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PlansController : ControllerBase
{
    /// <summary>
    /// Get all available plans
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<PlanInfo>), StatusCodes.Status200OK)]
    public IActionResult GetPlans()
    {
        var plans = new List<PlanInfo>
        {
            new PlanInfo(
                "Free",
                "Perfect for testing and small projects",
                100,
                0,
                new List<string>
                {
                    "100 requests/day",
                    "5 API keys",
                    "Basic toxicity detection",
                    "Community support"
                }
            ),
            new PlanInfo(
                "Starter",
                "Great for growing applications",
                1000,
                9.99m,
                new List<string>
                {
                    "1,000 requests/day",
                    "10 API keys",
                    "Full toxicity analysis",
                    "Webhook notifications",
                    "Email support"
                }
            ),
            new PlanInfo(
                "Pro",
                "For professional applications",
                10000,
                49.99m,
                new List<string>
                {
                    "10,000 requests/day",
                    "10 API keys",
                    "Full toxicity analysis",
                    "Webhook notifications",
                    "Priority support",
                    "Usage analytics",
                    "Batch processing"
                }
            ),
            new PlanInfo(
                "Enterprise",
                "Custom solutions for large organizations",
                int.MaxValue,
                -1, // Contact for pricing
                new List<string>
                {
                    "Unlimited requests",
                    "Unlimited API keys",
                    "Full toxicity analysis",
                    "Custom webhooks",
                    "Dedicated support",
                    "SLA guarantee",
                    "On-premise deployment option",
                    "Custom integrations"
                }
            )
        };
        
        return Ok(plans);
    }
    
    /// <summary>
    /// Get plan limits information
    /// </summary>
    [HttpGet("limits")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetLimits()
    {
        return Ok(new
        {
            free = new { dailyRequests = 100, apiKeys = 5 },
            starter = new { dailyRequests = 1000, apiKeys = 10 },
            pro = new { dailyRequests = 10000, apiKeys = 10 },
            enterprise = new { dailyRequests = "unlimited", apiKeys = "unlimited" }
        });
    }
}
