using MediatR;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LifeQuestAPI.Application.Features.UserTasks.Queries.GetUserTasks;
using LifeQuestAPI.Application.Features.UserTasks.Commands.CompleteUserTask;
using LifeQuestAPI.Application.Features.UserTasks.Commands.RequestNewQuests;
using LifeQuestAPI.Application.Features.UserTasks.Queries.GetDailyTaskPool;
using LifeQuestAPI.Application.Features.UserTasks.Commands.SelectTask;
using LifeQuestAPI.Application.Features.UserTasks.Commands.PostponeTask;
using LifeQuestAPI.Application.Features.UserTasks.Commands.PauseResumeTask;
using LifeQuestAPI.Application.Features.UserTasks.Commands.ShareUserTask;
using LifeQuestAPI.Application.Features.UserTasks.Queries.GetSharedTaskByToken;

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

    [HttpGet("available-tasks")]
    public async Task<IActionResult> GetAvailableTasks()
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString)) return Unauthorized();

        var userReadRepo = HttpContext.RequestServices.GetRequiredService<LifeQuestAPI.Application.Repositories.IAppUserReadRepository>();
        var taskReadRepo = HttpContext.RequestServices.GetRequiredService<LifeQuestAPI.Application.Repositories.IAppTaskReadRepository>();
        var userTaskReadRepo = HttpContext.RequestServices.GetRequiredService<LifeQuestAPI.Application.Repositories.IUserTaskReadRepository>();

        var userId = Guid.Parse(userIdString);
        var user = await userReadRepo.Table.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return NotFound();

        var assignedTaskIds = await userTaskReadRepo.Table
            .Where(ut => ut.AppUserId == userId && ut.Status != Domain.Enums.AppTaskStatus.Expired && ut.Status != Domain.Enums.AppTaskStatus.Abandoned)
            .Select(ut => ut.AppTaskId)
            .ToListAsync();

        var tasks = await taskReadRepo.Table
            .Include(t => t.Category)
            .Where(t => t.IsActive && t.MinLevel <= (int)Math.Floor(user.GeneralLevel) && !assignedTaskIds.Contains(t.Id))
            .ToListAsync();

        var result = tasks.Select(t => new
        {
            Id = t.Id,
            Title = t.Title,
            Description = t.Description,
            BaseXP = t.BaseXP,
            EstimatedMinutes = t.EstimatedMinutes.HasValue && t.EstimatedMinutes.Value > 0 
                ? t.EstimatedMinutes.Value 
                : (t.BaseXP > 0 ? (int)Math.Ceiling(t.BaseXP / 5.0) : 15),
            CategoryName = t.Category?.Name ?? "Genel",
            CategoryIcon = t.Category?.IconPath ?? "🎯",
            Difficulty = t.Difficulty.ToString(),
            MinLevel = t.MinLevel,
            WhyThisTask = t.WhyThisTask
        });

        return Ok(result);
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
    public async Task<IActionResult> CompleteTask([FromRoute] Guid id, [FromBody] CompleteTaskBody? body)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString)) return Unauthorized();

        var request = new CompleteUserTaskCommandRequest
        {
            UserTaskId = id,
            UserId = Guid.Parse(userIdString),
            ReflectionText = body?.ReflectionText,
            CustomTags = body?.CustomTags
        };

        var response = await _mediator.Send(request);

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    /// <summary>Görevi paylaşmak için token oluştur — FEAT-001</summary>
    [HttpPost("{id}/share")]
    public async Task<IActionResult> ShareTask([FromRoute] Guid id)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString)) return Unauthorized();

        var request = new ShareUserTaskCommandRequest
        {
            UserTaskId = id,
            UserId = Guid.Parse(userIdString)
        };

        var response = await _mediator.Send(request);
        if (!response.Success) return BadRequest(response);

        return Ok(response);
    }

    /// <summary>Paylaşılan görevi görüntüle (Login gerekmez) — FEAT-001</summary>
    [HttpGet("shared/{token}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetSharedTask([FromRoute] string token)
    {
        var request = new GetSharedTaskByTokenQueryRequest { Token = token };
        var response = await _mediator.Send(request);

        if (!response.Success) return NotFound(response);
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
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message, limitReached = ex.Message.Contains("limite") });
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

    /// <summary>Görev ertelemesini iptal et</summary>
    [HttpPost("{id}/cancel-postpone")]
    public async Task<IActionResult> CancelPostponeTask([FromRoute] Guid id)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString)) return Unauthorized();

        try
        {
            var success = await _mediator.Send(new LifeQuestAPI.Application.Features.UserTasks.Commands.CancelPostponeTask.CancelPostponeTaskCommandRequest
            {
                UserId = Guid.Parse(userIdString),
                UserTaskId = id
            });
            return Ok(new { success });
        }
        catch (Exception ex)
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

    /// <summary>Manuel olarak bugünün görev dağıtımını tetikle</summary>
    [HttpPost("trigger-daily-quests")]
    public async Task<IActionResult> TriggerDailyQuests()
    {
        var dailyQuestService = HttpContext.RequestServices.GetRequiredService<LifeQuestAPI.Application.Abstractions.DailyQuest.IDailyQuestService>();
        await dailyQuestService.DistributeDailyQuestsAsync();
        return Ok(new { success = true, message = "Günlük görev dağıtımı başarıyla manuel olarak tetiklendi." });
    }

    /// <summary>Ekstra görev ve rozet seed verilerini yükle</summary>
    [HttpPost("seed")]
    public async Task<IActionResult> TriggerSeed()
    {
        await LifeQuestAPI.Persistence.Contexts.SeedData.SeedData.Initialize(HttpContext.RequestServices);
        return Ok(new { success = true, message = "Seed verileri başarıyla yüklendi!" });
    }
}

// Request body record'ları
public record PostponeTaskBody(DateOnly? PostponeTo, string? SkipReason);
public record AbandonTaskBody(string? Reason);
public record CompleteTaskBody(string? ReflectionText, List<string>? CustomTags);