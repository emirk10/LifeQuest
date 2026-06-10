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
                JournalEntry = m.JournalEntry,
                SleepQuality = m.SleepQuality,
                RecordedAt = m.RecordedAt
            })
            .FirstOrDefaultAsync();

        return Ok(new { success = true, data = mood });
    }

    
    [HttpGet("history")]
    public async Task<IActionResult> GetMoodHistory([FromQuery] int page = 1, [FromQuery] int pageSize = 30)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString)) return Unauthorized();

        var userId = Guid.Parse(userIdString);

        var query = _moodReadRepository
            .GetWhere(m => m.AppUserId == userId, tracking: false);

        var totalCount = await query.CountAsync();

        var history = await query
            .OrderByDescending(m => m.Date)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(m => new UserDailyMoodDto
            {
                Id = m.Id,
                AppUserId = m.AppUserId,
                Date = m.Date,
                MoodMode = m.MoodMode,
                AvailableMinutes = m.AvailableMinutes,
                EnergyLevel = m.EnergyLevel,
                JournalEntry = m.JournalEntry,
                SleepQuality = m.SleepQuality,
                RecordedAt = m.RecordedAt
            })
            .ToListAsync();

        return Ok(new { success = true, data = history, totalCount, page, pageSize });
    }
}

public record RecordDailyMoodRequest(
    Domain.Enums.MoodMode MoodMode, 
    [System.ComponentModel.DataAnnotations.Range(1, 1440, ErrorMessage = "Müsaitlik süresi 1 ile 1440 dakika arasında olmalıdır.")] int AvailableMinutes, 
    [System.ComponentModel.DataAnnotations.Range(1, 10, ErrorMessage = "Enerji seviyesi 1 ile 10 arasında olmalıdır.")] int EnergyLevel, 
    string? JournalEntry, 
    [System.ComponentModel.DataAnnotations.Range(1, 5, ErrorMessage = "Uyku kalitesi 1 ile 5 arasında olmalıdır.")] int? SleepQuality
);
