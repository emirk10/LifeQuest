using System.Security.Claims;
using LifeQuestAPI.Application.Features.Goals.Commands.CreateUserGoal;
using LifeQuestAPI.Application.Features.Goals.Commands.UpdateGoalProgress;
using LifeQuestAPI.Application.Features.Goals.Commands.ArchiveGoal;
using LifeQuestAPI.Application.Features.Goals.Queries.GetUserGoals;
using LifeQuestAPI.Application.Features.Goals.Queries.GetGoalDashboard;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LifeQuestAPI.API.Controllers;

[Route("api/goals")]
[ApiController]
[Authorize]
public class GoalsController : ControllerBase
{
    private readonly IMediator _mediator;

    public GoalsController(IMediator mediator) => _mediator = mediator;

    private Guid GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (claim == null || !Guid.TryParse(claim.Value, out var userId))
            throw new UnauthorizedAccessException("Geçersiz kullanıcı kimliği.");
        return userId;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyGoals()
    {
        var response = await _mediator.Send(new GetUserGoalsQueryRequest { UserId = GetUserId() });
        return Ok(response);
    }

    /// <summary>SP-05: Hedef dashboard (aktif/tamamlanan/arşiv + lag detection)</summary>
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var response = await _mediator.Send(new GetGoalDashboardQueryRequest { UserId = GetUserId() });
        return Ok(response);
    }

    [HttpPost]
    public async Task<IActionResult> CreateGoal([FromBody] CreateGoalBody body)
    {
        var request = new CreateUserGoalCommandRequest
        {
            UserId = GetUserId(),
            Title = body.Title,
            Description = body.Description,
            Type = body.Type,
            TargetValue = body.TargetValue,
            CategoryId = body.CategoryId,
            StartDate = body.StartDate,
            EndDate = body.EndDate,
            BonusXP = body.BonusXP,
            // SP-05 alanlar
            Icon = body.Icon,
            Color = body.Color,
            Why = body.Why,
            ProgressUnit = body.ProgressUnit ?? "görev",
            DifficultyLevel = body.DifficultyLevel,
            AutoTrackByCategory = body.AutoTrackByCategory,
            AutoTrackByXP = body.AutoTrackByXP,
            ReminderEnabled = body.ReminderEnabled,
            ReminderFrequency = body.ReminderFrequency ?? "daily"
        };

        var response = await _mediator.Send(request);
        return Ok(response);
    }

    /// <summary>SP-05: Manuel ilerleme güncelle (+1 / +5 / +10)</summary>
    [HttpPatch("{goalId}/progress")]
    public async Task<IActionResult> UpdateProgress(Guid goalId, [FromBody] UpdateProgressBody body)
    {
        try
        {
            var response = await _mediator.Send(new UpdateGoalProgressCommandRequest
            {
                UserId = GetUserId(),
                GoalId = goalId,
                IncrementBy = body.IncrementBy,
                Note = body.Note
            });
            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    /// <summary>SP-05: Hedefi arşivle</summary>
    [HttpPatch("{goalId}/archive")]
    public async Task<IActionResult> ArchiveGoal(Guid goalId)
    {
        try
        {
            var result = await _mediator.Send(new ArchiveGoalCommandRequest
            {
                UserId = GetUserId(),
                GoalId = goalId
            });
            return Ok(new { success = result });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpPut("{goalId}")]
    public async Task<IActionResult> UpdateGoal(Guid goalId, [FromBody] CreateGoalBody body)
    {
        try
        {
            var response = await _mediator.Send(new LifeQuestAPI.Application.Features.Goals.Commands.UpdateUserGoal.UpdateUserGoalCommandRequest
            {
                GoalId = goalId,
                UserId = GetUserId(),
                Title = body.Title,
                Description = body.Description,
                Type = body.Type,
                TargetValue = body.TargetValue,
                CategoryId = body.CategoryId,
                StartDate = body.StartDate,
                EndDate = body.EndDate,
                BonusXP = body.BonusXP,
                Icon = body.Icon,
                Color = body.Color,
                Why = body.Why,
                ProgressUnit = body.ProgressUnit ?? "görev",
                AutoTrackByCategory = body.AutoTrackByCategory,
                AutoTrackByXP = body.AutoTrackByXP,
                ReminderEnabled = body.ReminderEnabled,
                ReminderFrequency = body.ReminderFrequency ?? "weekly"
            });
            if (!response.IsSuccess) return NotFound(response);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpDelete("{goalId}")]
    public async Task<IActionResult> DeleteGoal(Guid goalId)
    {
        try
        {
            var response = await _mediator.Send(new LifeQuestAPI.Application.Features.Goals.Commands.DeleteUserGoal.DeleteUserGoalCommandRequest
            {
                GoalId = goalId,
                UserId = GetUserId()
            });
            if (!response.IsSuccess) return NotFound(response);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }
}

public class CreateGoalBody
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public LifeQuestAPI.Domain.Enums.GoalType Type { get; set; }
    public int TargetValue { get; set; }
    public Guid? CategoryId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int BonusXP { get; set; }
    // SP-05
    public string? Icon { get; set; }
    public string? Color { get; set; }
    public string? Why { get; set; }
    public string? ProgressUnit { get; set; }
    public int DifficultyLevel { get; set; } = 2;
    public bool AutoTrackByCategory { get; set; }
    public bool AutoTrackByXP { get; set; }
    public bool ReminderEnabled { get; set; } = true;
    public string? ReminderFrequency { get; set; }
}

public class UpdateProgressBody
{
    public int IncrementBy { get; set; } = 1;
    public string? Note { get; set; }
}
