using LifeQuestAPI.Application.Features.User.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using LifeQuestAPI.Application.Features.User.Queries.GetLeaderboard;
using LifeQuestAPI.Application.Features.User.Queries.GetUserProfile;
using LifeQuestAPI.Application.Features.User.Queries.GetUserStats;
using LifeQuestAPI.Application.Features.User.Queries.GetUserActivities;
using LifeQuestAPI.Application.Features.User.Queries.GetUserBehaviorSummary;
using LifeQuestAPI.Application.Features.User.Commands.Vacation;
using LifeQuestAPI.Application.Features.User.Commands.UpdateGoals;
using LifeQuestAPI.Application.Repositories;

namespace LifeQuestAPI.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IAppUserReadRepository _userReadRepository;
    private readonly IAppUserWriteRepository _userWriteRepository;

    public UsersController(
        IMediator mediator,
        IAppUserReadRepository userReadRepository,
        IAppUserWriteRepository userWriteRepository)
    {
        _mediator = mediator;
        _userReadRepository = userReadRepository;
        _userWriteRepository = userWriteRepository;
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetMyProfile()
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString)) return Unauthorized();
        var response = await _mediator.Send(new GetUserProfileQueryRequest { UserId = Guid.Parse(userIdString) });
        return Ok(response);
    }

    [HttpGet("leaderboard")]
    [AllowAnonymous]
    public async Task<IActionResult> GetLeaderboard([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] string filter = "global")
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        Guid? currentUserId = string.IsNullOrEmpty(userIdString) ? null : Guid.Parse(userIdString);

        var request = new GetLeaderboardQueryRequest(
            PageNumber: pageNumber,
            PageSize: pageSize,
            Filter: filter,
            CurrentUserId: currentUserId
        );

        var response = await _mediator.Send(request);
        return Ok(response);
    }

    [HttpGet("activities")]
    public async Task<IActionResult> GetActivities()
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString)) return Unauthorized();
        var response = await _mediator.Send(new GetUserActivitiesQueryRequest { UserId = Guid.Parse(userIdString) });
        return Ok(response);
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString)) return Unauthorized();
        var response = await _mediator.Send(new GetUserStatsQueryRequest { UserId = Guid.Parse(userIdString) });
        return Ok(response);
    }

    
    [HttpPatch("{id}/chat-personality")]
    public async Task<IActionResult> UpdateChatPersonality(Guid id, [FromBody] ChatPersonalityBody body)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString) || Guid.Parse(userIdString) != id) return Unauthorized();

        var user = await _userReadRepository.GetByIdAsync(id.ToString(), tracking: true);
        if (user == null) return NotFound();

        var valid = new[] { "friendly", "motivator", "coach" };
        if (!valid.Contains(body.ChatPersonality)) return BadRequest("Geçersiz kişilik değeri.");

        user.ChatPersonality = body.ChatPersonality;
        user.UpdatedAt = DateTime.UtcNow;
        _userWriteRepository.Update(user);
        await _userWriteRepository.SaveAsync();
        return Ok(new { success = true });
    }

    
    [HttpPatch("{id}/nudge-settings")]
    public async Task<IActionResult> UpdateNudgeSettings(Guid id, [FromBody] NudgeSettingsBody body)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString) || Guid.Parse(userIdString) != id) return Unauthorized();

        var user = await _userReadRepository.GetByIdAsync(id.ToString(), tracking: true);
        if (user == null) return NotFound();

        user.NudgeEnabled = body.NudgeEnabled;
        user.UpdatedAt = DateTime.UtcNow;
        _userWriteRepository.Update(user);
        await _userWriteRepository.SaveAsync();
        return Ok(new { success = true });
    }

    
    [HttpGet("{id}/behavior-summary")]
    public async Task<IActionResult> GetBehaviorSummary(Guid id)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString) || Guid.Parse(userIdString) != id) return Unauthorized();

        var response = await _mediator.Send(new GetUserBehaviorSummaryQueryRequest { UserId = id });
        return Ok(response);
    }

    
    [HttpPost("{id}/vacation/start")]
    public async Task<IActionResult> StartVacation(Guid id, [FromBody] StartVacationBody body)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString) || Guid.Parse(userIdString) != id) return Unauthorized();

        var response = await _mediator.Send(new StartVacationCommandRequest
        {
            UserId  = id,
            EndDate = body.EndDate,
            Reason  = body.Reason
        });
        return response.Success ? Ok(response) : BadRequest(response);
    }

    
    [HttpPost("{id}/vacation/end")]
    public async Task<IActionResult> EndVacation(Guid id)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString) || Guid.Parse(userIdString) != id) return Unauthorized();

        var response = await _mediator.Send(new EndVacationCommandRequest { UserId = id });
        return response.Success ? Ok(response) : BadRequest(response);
    }

    
    [HttpPut("{id}/goals")]
    public async Task<IActionResult> UpdateGoals(Guid id, [FromBody] UpdateGoalsBody body)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString) || Guid.Parse(userIdString) != id) return Unauthorized();

        var response = await _mediator.Send(new UpdateUserGoalsCommandRequest
        {
            UserId       = id,
            NewGoals     = body.NewGoals,
            RemovedGoals = body.RemovedGoals
        });
        return response.Success ? Ok(response) : BadRequest(response);
    }

    
    [HttpPatch("{id}/preferences")]
    public async Task<IActionResult> UpdatePreferences(Guid id, [FromBody] UpdatePreferencesBody body)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString) || Guid.Parse(userIdString) != id) return Unauthorized();

        var user = await _userReadRepository.GetByIdAsync(id.ToString(), tracking: true);
        if (user == null) return NotFound();

        if (body.Goals != null) user.UserGoals = body.Goals;
        if (body.DailyAvailableMinutes.HasValue) user.DailyAvailableMinutes = body.DailyAvailableMinutes;
        if (body.Strengths != null) user.Strengths = body.Strengths;
        if (body.Weaknesses != null) user.Weaknesses = body.Weaknesses;
        if (body.MotivationFactor != null) user.MotivationFactor = body.MotivationFactor;
        if (body.PreferredTasksPerWeek.HasValue) user.PreferredTasksPerWeek = body.PreferredTasksPerWeek;
        if (body.DifficultyPreference != null) user.DifficultyPreference = body.DifficultyPreference;
        if (body.Bio != null) user.Bio = body.Bio;
        if (body.Location != null) user.Location = body.Location;
        if (body.Interests != null) user.Interests = body.Interests;

        user.UpdatedAt = DateTime.UtcNow;
        _userWriteRepository.Update(user);
        await _userWriteRepository.SaveAsync();

        return Ok(new { success = true, message = "Tercihleriniz başarıyla güncellendi." });
    }
}

public class ChatPersonalityBody { public string ChatPersonality { get; set; } = "friendly"; }
public class NudgeSettingsBody { public bool NudgeEnabled { get; set; } = true; }
public class StartVacationBody { public DateOnly? EndDate { get; set; } public string? Reason { get; set; } }
public class UpdateGoalsBody { public List<string> NewGoals { get; set; } = new(); public List<string> RemovedGoals { get; set; } = new(); }
public class UpdatePreferencesBody
{
    public List<string>? Goals { get; set; }

    [System.ComponentModel.DataAnnotations.Range(0, 1440, ErrorMessage = "Günlük müsaitlik süresi 0 ile 1440 dakika arasında olmalıdır.")]
    public int? DailyAvailableMinutes { get; set; }
    public List<string>? Strengths { get; set; }
    public List<string>? Weaknesses { get; set; }
    public string? MotivationFactor { get; set; }

    [System.ComponentModel.DataAnnotations.Range(0, 100, ErrorMessage = "Haftalık tercih edilen görev sayısı 0 ile 100 arasında olmalıdır.")]
    public int? PreferredTasksPerWeek { get; set; }
    public string? DifficultyPreference { get; set; }
    public string? Bio { get; set; }
    public string? Location { get; set; }
    public List<string>? Interests { get; set; }
}