using MediatR;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using LifeQuestAPI.Application.Features.Mood.Commands.RecordDailyMood;
using LifeQuestAPI.Application.Repositories;
using LifeQuestAPI.Application.DTOs;
using Microsoft.EntityFrameworkCore;

namespace LifeQuestAPI.API.Controllers;

[Route("api/mood")]
[ApiController]
[Authorize]
public class MoodController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IUserDailyMoodReadRepository _moodReadRepository;

    public MoodController(IMediator mediator, IUserDailyMoodReadRepository moodReadRepository)
    {
        _mediator = mediator;
        _moodReadRepository = moodReadRepository;
    }

    /// <summary>POST /api/mood/daily — Günlük mood kaydet (UPSERT)</summary>
    [HttpPost("daily")]
    public async Task<IActionResult> RecordDailyMood([FromBody] RecordDailyMoodRequest request)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString)) return Unauthorized();

        var result = await _mediator.Send(new RecordDailyMoodCommandRequest
        {
            UserId = Guid.Parse(userIdString),
            MoodMode = request.MoodMode,
            AvailableMinutes = request.AvailableMinutes,
            EnergyLevel = request.EnergyLevel
        });

        return Ok(new { success = true, data = result });
    }

    /// <summary>GET /api/mood/today — Bugünkü mood kaydını getir</summary>
    [HttpGet("today")]
    public async Task<IActionResult> GetTodayMood()
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString)) return Unauthorized();

        var userId = Guid.Parse(userIdString);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var mood = await _moodReadRepository
            .GetWhere(m => m.AppUserId == userId && m.Date == today, tracking: false)
            .Select(m => new UserDailyMoodDto
            {
                Id = m.Id,
                AppUserId = m.AppUserId,
                Date = m.Date,
                MoodMode = m.MoodMode,
                AvailableMinutes = m.AvailableMinutes,
                EnergyLevel = m.EnergyLevel,
                RecordedAt = m.RecordedAt
            })
            .FirstOrDefaultAsync();

        return Ok(new { success = true, data = mood });
    }

    /// <summary>GET /api/mood/history — Son 30 günün mood geçmişi</summary>
    [HttpGet("history")]
    public async Task<IActionResult> GetMoodHistory()
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString)) return Unauthorized();

        var userId = Guid.Parse(userIdString);
        var thirtyDaysAgo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30));

        var history = await _moodReadRepository
            .GetWhere(m => m.AppUserId == userId && m.Date >= thirtyDaysAgo, tracking: false)
            .OrderByDescending(m => m.Date)
            .Select(m => new UserDailyMoodDto
            {
                Id = m.Id,
                AppUserId = m.AppUserId,
                Date = m.Date,
                MoodMode = m.MoodMode,
                AvailableMinutes = m.AvailableMinutes,
                EnergyLevel = m.EnergyLevel,
                RecordedAt = m.RecordedAt
            })
            .ToListAsync();

        return Ok(new { success = true, data = history });
    }
}

public record RecordDailyMoodRequest(Domain.Enums.MoodMode MoodMode, int AvailableMinutes, int EnergyLevel);
