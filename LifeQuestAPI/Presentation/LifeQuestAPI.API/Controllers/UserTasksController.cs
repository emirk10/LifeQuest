using MediatR;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using LifeQuestAPI.Application.Features.UserTasks.Queries.GetUserTasks;
using LifeQuestAPI.Application.Features.UserTasks.Commands.CompleteUserTask;
using LifeQuestAPI.Application.Features.UserTasks.Commands.RequestNewQuests;
using LifeQuestAPI.Application.Features.UserTasks.Queries.GetDailyTaskPool;
using LifeQuestAPI.Application.Features.UserTasks.Commands.SelectTask;
using LifeQuestAPI.Application.Features.UserTasks.Commands.PostponeTask;
using LifeQuestAPI.Application.Features.UserTasks.Commands.PauseResumeTask;

namespace LifeQuestAPI.API.Controllers;

[Route("api/user-tasks")]
[ApiController]
[Authorize]
public class UserTasksController : ControllerBase
{
    private readonly IMediator _mediator;

    public UserTasksController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetUserTasks([FromQuery] Guid? categoryId)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString)) return Unauthorized();

        var request = new GetUserTasksQueryRequest
        {
            UserId = Guid.Parse(userIdString),
            CategoryId = categoryId
        };

        var response = await _mediator.Send(request);
        return Ok(response);
    }

    [HttpPatch("{id}/complete")]
    public async Task<IActionResult> CompleteTask([FromRoute] Guid id)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString)) return Unauthorized();

        var request = new CompleteUserTaskCommandRequest
        {
            UserTaskId = id,
            UserId = Guid.Parse(userIdString)
        };

        var response = await _mediator.Send(request);

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    [HttpPost("request-new")]
    public async Task<IActionResult> RequestNewQuests()
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString)) return Unauthorized();

        var request = new RequestNewQuestsCommandRequest { UserId = Guid.Parse(userIdString) };
        var response = await _mediator.Send(request);

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    [HttpPost("assign/{appTaskId}")]
    public async Task<IActionResult> AssignSpecificTask([FromRoute] Guid appTaskId)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString)) return Unauthorized();

        var request = new LifeQuestAPI.Application.Features.UserTasks.Commands.AssignSpecificTask.AssignSpecificTaskCommandRequest
        {
            UserId = Guid.Parse(userIdString),
            AppTaskId = appTaskId
        };

        var response = await _mediator.Send(request);

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    // ─── SP-01: Yeni Endpoint'ler ─────────────────────────────────────────

    /// <summary>Bugünkü görev havuzunu getir (6 öneri, mood-duyarlı)</summary>
    [HttpGet("daily-pool")]
    public async Task<IActionResult> GetDailyPool([FromQuery] int count = 6)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString)) return Unauthorized();

        var response = await _mediator.Send(new GetDailyTaskPoolQueryRequest
        {
            UserId = Guid.Parse(userIdString),
            Count = count
        });
        return Ok(response);
    }

    /// <summary>Havuzdan görev seç (Suggested → Selected)</summary>
    [HttpPost("{id}/select")]
    public async Task<IActionResult> SelectTask([FromRoute] Guid id, [FromQuery] bool forceOverLimit = false)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString)) return Unauthorized();

        try
        {
            var response = await _mediator.Send(new SelectTaskCommandRequest
            {
                UserId = Guid.Parse(userIdString),
                UserTaskId = id,
                ForceOverLimit = forceOverLimit
            });
            return Ok(response);
        }
        catch (Exception ex) when (ex.Message.Contains("limite"))
        {
            return BadRequest(new { success = false, message = ex.Message, limitReached = true });
        }
    }


    /// <summary>Görevi ertele</summary>
    [HttpPost("{id}/postpone")]
    public async Task<IActionResult> PostponeTask([FromRoute] Guid id, [FromBody] PostponeTaskBody body)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString)) return Unauthorized();

        try
        {
            var response = await _mediator.Send(new PostponeTaskCommandRequest
            {
                UserId = Guid.Parse(userIdString),
                UserTaskId = id,
                PostponeTo = body.PostponeTo ?? DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
                SkipReason = body.SkipReason
            });
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    /// <summary>Görevi duraklat</summary>
    [HttpPost("{id}/pause")]
    public async Task<IActionResult> PauseTask([FromRoute] Guid id)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString)) return Unauthorized();

        var response = await _mediator.Send(new PauseTaskCommandRequest
        {
            UserId = Guid.Parse(userIdString),
            UserTaskId = id
        });
        return Ok(response);
    }

    /// <summary>Görevi devam ettir</summary>
    [HttpPost("{id}/resume")]
    public async Task<IActionResult> ResumeTask([FromRoute] Guid id)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString)) return Unauthorized();

        var response = await _mediator.Send(new ResumeTaskCommandRequest
        {
            UserId = Guid.Parse(userIdString),
            UserTaskId = id
        });
        return Ok(response);
    }

    /// <summary>Görevden vazgeç</summary>
    [HttpPost("{id}/abandon")]
    public async Task<IActionResult> AbandonTask([FromRoute] Guid id, [FromBody] AbandonTaskBody body)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString)) return Unauthorized();

        var response = await _mediator.Send(new AbandonTaskCommandRequest
        {
            UserId = Guid.Parse(userIdString),
            UserTaskId = id,
            Reason = body.Reason
        });
        return Ok(new { success = response });
    }
}

// Request body record'ları
public record PostponeTaskBody(DateOnly? PostponeTo, string? SkipReason);
public record AbandonTaskBody(string? Reason);