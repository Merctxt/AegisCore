using AegisCoreApi.Data;
using AegisCoreApi.DTOs;
using AegisCoreApi.Models;
using Microsoft.EntityFrameworkCore;

namespace AegisCoreApi.Services;

public interface IRequestLogService
{
    Task LogRequestAsync(RequestLog log);
    Task<UsageStatsResponse> GetUsageStatsAsync(Guid userId);
    Task<List<RequestLog>> GetRecentLogsAsync(Guid userId, int count = 50);
}

public class RequestLogService : IRequestLogService
{
    private readonly AegisDbContext _context;
    
    public RequestLogService(AegisDbContext context)
    {
        _context = context;
    }
    
    public async Task LogRequestAsync(RequestLog log)
    {
        _context.RequestLogs.Add(log);
        await _context.SaveChangesAsync();
    }
    
    public async Task<UsageStatsResponse> GetUsageStatsAsync(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return new UsageStatsResponse(0, 0, 100, 0, new List<DailyUsage>());
        }
        
        var today = DateTime.UtcNow.Date;
        var thirtyDaysAgo = today.AddDays(-30);
        
        var logs = await _context.RequestLogs
            .Where(r => r.UserId == userId && r.CreatedAt >= thirtyDaysAgo)
            .ToListAsync();
            
        var requestsToday = logs.Count(r => r.CreatedAt.Date == today);
        var requestsThisMonth = logs.Count;
        
        var dailyLimit = GetDailyLimit(user.Plan);
        var usagePercentage = dailyLimit > 0 ? (double)requestsToday / dailyLimit * 100 : 0;
        
        var last30Days = Enumerable.Range(0, 30)
            .Select(i => today.AddDays(-i))
            .Select(date => new DailyUsage(
                date,
                logs.Count(r => r.CreatedAt.Date == date),
                logs.Count(r => r.CreatedAt.Date == date && r.IsToxic == true)
            ))
            .OrderBy(d => d.Date)
            .ToList();
            
        return new UsageStatsResponse(
            requestsToday,
            requestsThisMonth,
            dailyLimit,
            Math.Round(usagePercentage, 2),
            last30Days
        );
    }
    
    public async Task<List<RequestLog>> GetRecentLogsAsync(Guid userId, int count = 50)
    {
        return await _context.RequestLogs
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .Take(count)
            .ToListAsync();
    }
    
    private static int GetDailyLimit(PlanType plan) => plan switch
    {
        PlanType.Free => 100,
        PlanType.Starter => 1000,
        PlanType.Pro => 10000,
        PlanType.Enterprise => int.MaxValue,
        _ => 100
    };
}
