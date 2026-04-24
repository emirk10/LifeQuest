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
    public async Task<IActionResult> GetLeaderboard()
        => Ok(await _mediator.Send(new GetLeaderboardQueryRequest()));

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

    /// <summary>SP-02: Chatbot kişiliğini güncelle (friendly | motivator | coach).</summary>
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

    /// <summary>SP-02: Nudge bildirimlerini aç/kapat.</summary>
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

    /// <summary>SP-03: Kullanıcının son 30 gün davranış analizi özeti.</summary>
    [HttpGet("{id}/behavior-summary")]
    public async Task<IActionResult> GetBehaviorSummary(Guid id)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString) || Guid.Parse(userIdString) != id) return Unauthorized();

        var response = await _mediator.Send(new GetUserBehaviorSummaryQueryRequest { UserId = id });
        return Ok(response);
    }

    /// <summary>SP-04: Tatil/mola modunu başlat.</summary>
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

    /// <summary>SP-04: Tatil/mola modunu bitir.</summary>
    [HttpPost("{id}/vacation/end")]
    public async Task<IActionResult> EndVacation(Guid id)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString) || Guid.Parse(userIdString) != id) return Unauthorized();

        var response = await _mediator.Send(new EndVacationCommandRequest { UserId = id });
        return response.Success ? Ok(response) : BadRequest(response);
    }

    /// <summary>SP-04: Kullanıcı hedeflerini güncelle.</summary>
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
}

public class ChatPersonalityBody { public string ChatPersonality { get; set; } = "friendly"; }
public class NudgeSettingsBody { public bool NudgeEnabled { get; set; } = true; }
public class StartVacationBody { public DateOnly? EndDate { get; set; } public string? Reason { get; set; } }
public class UpdateGoalsBody { public List<string> NewGoals { get; set; } = new(); public List<string> RemovedGoals { get; set; } = new(); }