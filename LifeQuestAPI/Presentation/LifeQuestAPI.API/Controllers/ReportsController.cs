using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;
using LifeQuestAPI.Application.Repositories;
using LifeQuestAPI.Application.Abstractions.Notifications;
using LifeQuestAPI.Application.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LifeQuestAPI.API.Controllers;

[Route("api/reports")]
[ApiController]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IWeeklyReportReadRepository _reportReadRepository;
    private readonly IWeeklyReportWriteRepository _reportWriteRepository;
    private readonly IUserTaskReadRepository _userTaskReadRepository;
    private readonly IAppUserReadRepository _userReadRepository;
    private readonly IAppNotificationService _notificationService;
    private readonly IUserBehaviorAnalysisService _behaviorService;

    public ReportsController(
        IWeeklyReportReadRepository reportReadRepository,
        IWeeklyReportWriteRepository reportWriteRepository,
        IUserTaskReadRepository userTaskReadRepository,
        IAppUserReadRepository userReadRepository,
        IAppNotificationService notificationService,
        IUserBehaviorAnalysisService behaviorService)
    {
        _reportReadRepository = reportReadRepository;
        _reportWriteRepository = reportWriteRepository;
        _userTaskReadRepository = userTaskReadRepository;
        _userReadRepository = userReadRepository;
        _notificationService = notificationService;
        _behaviorService = behaviorService;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyReports()
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString)) return Unauthorized();

        var userId = Guid.Parse(userIdString);

        var reports = await _reportReadRepository.Table
            .Where(r => r.AppUserId == userId)
            .OrderByDescending(r => r.WeekEndDate)
            .Take(10)
            .ToListAsync();

        return Ok(new { success = true, reports });
    }

    [HttpGet("daily-breakdown")]
    public async Task<IActionResult> GetDailyBreakdown()
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString)) return Unauthorized();

        var userId = Guid.Parse(userIdString);

        var today = DateTime.UtcNow.Date;
        var startDay = today.AddDays(-6);

        var userTasks = await _userTaskReadRepository.Table
            .Where(ut => ut.AppUserId == userId && ut.Status == Domain.Enums.AppTaskStatus.Completed && ut.CompletedAt != null && ut.CompletedAt >= startDay)
            .ToListAsync();

        var dailyData = new List<object>();

        for (int i = 6; i >= 0; i--)
        {
            var day = today.AddDays(-i);
            var completedThatDay = userTasks
                .Where(ut => ut.CompletedAt.HasValue && ut.CompletedAt.Value.Date == day.Date)
                .ToList();

            dailyData.Add(new
            {
                date = day.ToString("yyyy-MM-dd"),
                XPKazanci = completedThatDay.Sum(ut => ut.EarnedXp),
                Tamamlanan = completedThatDay.Count
            });
        }

        return Ok(new { success = true, breakdown = dailyData });
    }

    [HttpPost("trigger-weekly-report")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> TriggerWeeklyReport()
    {
        var weekEnd = DateTime.UtcNow;
        var weekStart = weekEnd.AddDays(-7);

        var users = await _userReadRepository.GetAll(false).ToListAsync();

        int generatedCount = 0;
        foreach (var user in users)
        {
            var thisWeekTasks = await _userTaskReadRepository.Table
                .Include(ut => ut.AppTask)
                .Where(ut => ut.AppUserId == user.Id && ut.AssignedAt >= weekStart)
                .ToListAsync();

            if (!thisWeekTasks.Any()) continue;

            var completedTasks = thisWeekTasks.Where(t => t.Status == Domain.Enums.AppTaskStatus.Completed).ToList();
            var failedTasks = thisWeekTasks.Where(t => t.Status == Domain.Enums.AppTaskStatus.Abandoned
                || t.Status == Domain.Enums.AppTaskStatus.Expired
                || (t.DueDate < DateTime.UtcNow && t.Status != Domain.Enums.AppTaskStatus.Completed)).ToList();

            var totalXpEarned = completedTasks.Sum(t => t.EarnedXp);

            string insight = "Bu hafta harika bir iş çıkardın!";
            string recommendation = "Önümüzdeki hafta Sağlık kategorisine daha fazla odaklanabilirsin.";

            var categoryGroups = completedTasks
                .Where(t => t.AppTask != null)
                .GroupBy(t => t.AppTask.CategoryId)
                .Select(g => new { CategoryId = g.Key, Count = g.Count() })
                .OrderByDescending(g => g.Count)
                .ToList();

            var report = new Domain.Entities.WeeklyReport
            {
                Id = Guid.NewGuid(),
                AppUserId = user.Id,
                WeekStartDate = weekStart,
                WeekEndDate = weekEnd,
                TasksCompleted = completedTasks.Count,
                TasksFailed = failedTasks.Count,
                TotalXPEarned = totalXpEarned,
                StreakDays = 0,
                StrongestCategory = categoryGroups.FirstOrDefault()?.CategoryId.ToString() ?? "N/A",
                WeakestCategory = categoryGroups.LastOrDefault()?.CategoryId.ToString() ?? "N/A",
                AIInsight = insight,
                AIRecommendation = recommendation,
                OverallPerformanceScore = (thisWeekTasks.Count > 0) ? ((float)completedTasks.Count / thisWeekTasks.Count * 100) : 0,
                CategoryBreakdownJson = System.Text.Json.JsonSerializer.Serialize(categoryGroups),
                CreatedAt = DateTime.UtcNow
            };

            await _reportWriteRepository.AddAsync(report);
            generatedCount++;

            await _notificationService.SendNotificationAsync(
                user.Id, 
                "Haftalık Raporun Hazır! 📊", 
                $"Bu hafta {completedTasks.Count} görev tamamladın ve toplam {totalXpEarned} XP kazandın.", 
                "Report");

            try { await _behaviorService.AnalyzeAndUpdateUserBehaviorAsync(user.Id); }
            catch { }
        }

        await _reportWriteRepository.SaveAsync();

        return Ok(new { success = true, message = $"{generatedCount} kullanıcı için haftalık rapor başarıyla üretildi!" });
    }
}