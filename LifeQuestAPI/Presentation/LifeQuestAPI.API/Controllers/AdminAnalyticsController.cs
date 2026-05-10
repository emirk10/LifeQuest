using LifeQuestAPI.Application.Repositories;
using LifeQuestAPI.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LifeQuestAPI.API.Controllers;

[Route("api/admin/analytics")]
[ApiController]
[Authorize(Roles = "Admin")]
public class AdminAnalyticsController : ControllerBase
{
    private readonly IUserTaskReadRepository _userTaskRead;
    private readonly IAppUserReadRepository _userRead;
    private readonly ICategoryReadRepository _categoryRead;

    public AdminAnalyticsController(
        IUserTaskReadRepository userTaskRead,
        IAppUserReadRepository userRead,
        ICategoryReadRepository categoryRead)
    {
        _userTaskRead = userTaskRead;
        _userRead = userRead;
        _categoryRead = categoryRead;
    }

    [HttpGet("task-dropout")]
    public async Task<IActionResult> GetTaskDropout()
    {
        var stats = await _userTaskRead.Table
            .GroupBy(ut => ut.Status)
            .Select(g => new { Status = g.Key.ToString(), Count = g.Count() })
            .ToListAsync();

        return Ok(stats);
    }

    [HttpGet("category-performance")]
    public async Task<IActionResult> GetCategoryPerformance()
    {
        var stats = await _userTaskRead.Table
            .Include(ut => ut.AppTask)
                .ThenInclude(t => t.Category)
            .Where(ut => ut.Status == AppTaskStatus.Completed)
            .GroupBy(ut => ut.AppTask.Category.Name)
            .Select(g => new { Category = g.Key, Count = g.Count() })
            .ToListAsync();

        return Ok(stats);
    }

    [HttpGet("hourly-completions")]
    public async Task<IActionResult> GetHourlyCompletions()
    {
        var stats = await _userTaskRead.Table
            .Where(ut => ut.Status == AppTaskStatus.Completed && ut.CompletedAt.HasValue)
            .GroupBy(ut => ut.CompletedAt!.Value.Hour)
            .Select(g => new { Hour = g.Key, Count = g.Count() })
            .OrderBy(x => x.Hour)
            .ToListAsync();

        return Ok(stats);
    }

    [HttpGet("user-retention")]
    public async Task<IActionResult> GetUserRetention()
    {
        var today = DateTime.UtcNow.Date;
        var last7Days = today.AddDays(-7);

        var activeUsers = await _userRead.Table
            .CountAsync(u => u.LastTaskCompletedDate >= DateOnly.FromDateTime(last7Days));
        
        var totalUsers = await _userRead.Table.CountAsync();

        return Ok(new
        {
            TotalUsers = totalUsers,
            ActiveUsersLast7Days = activeUsers,
            RetentionRate = totalUsers > 0 ? (double)activeUsers / totalUsers : 0
        });
    }
}
