using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using System.Threading.Tasks;
using LifeQuestAPI.Application.Features.Admin.Commands.WipeAndSeed;
using System.Collections.Generic;
using System.Linq;

namespace LifeQuestAPI.API.Controllers;

[Route("api/admin")]
[ApiController]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("wipe-and-seed")]
    public async Task<IActionResult> WipeAndSeed([FromBody] WipeAndSeedRequest request)
    {
        if (request == null || (request.Categories == null && request.Badges == null && request.Achievements == null))
            return BadRequest(new { success = false, message = "Geçersiz veya boş veri!" });

        var mediatRRequest = new WipeAndSeedCommandRequest
        {
            WipeDatabase = request.WipeDatabase,
            Categories = request.Categories?.Select(c => new Application.Abstractions.CategorySeedRequestDto
            {
                Name = c.Name,
                Description = c.Description,
                IconPath = c.IconPath
            }).ToList(),
            Tasks = request.Tasks?.Select(t => new Application.Abstractions.TaskSeedRequestDto
            {
                Title = t.Title,
                Description = t.Description,
                CategoryName = t.CategoryName,
                BaseXP = t.BaseXP,
                MinLevel = t.MinLevel,
                MaxLevel = t.MaxLevel,
                Difficulty = (int)t.Difficulty,
                IsDaily = t.IsDaily,
                EstimatedMinutes = t.EstimatedMinutes,
                WhyThisTask = t.WhyThisTask
            }).ToList(),
            Badges = request.Badges?.Select(b => new Application.Abstractions.BadgeSeedRequestDto
            {
                Name = b.Name,
                Description = b.Description,
                IconPath = b.IconPath,
                RequiredLevel = b.RequiredLevel
            }).ToList(),
            Achievements = request.Achievements?.Select(a => new Application.Abstractions.AchievementSeedRequestDto
            {
                Title = a.Title,
                Description = a.Description,
                IconPath = a.IconPath,
                Type = (int)a.Type,
                RequiredCount = a.RequiredCount,
                BonusXP = a.BonusXP,
                IsHidden = a.IsHidden
            }).ToList()
        };

        var response = await _mediator.Send(mediatRRequest);
        if (!response.Success)
            return BadRequest(new { success = false, message = response.Message });

        return Ok(new { success = true, message = response.Message });
    }
}

public class WipeAndSeedRequest
{
    public bool WipeDatabase { get; set; } = false;
    public List<CategorySeedDto>? Categories { get; set; }
    public List<TaskSeedDto>? Tasks { get; set; }
    public List<BadgeSeedDto>? Badges { get; set; }
    public List<AchievementSeedDto>? Achievements { get; set; }
}

public class CategorySeedDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? IconPath { get; set; }
}

public class TaskSeedDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public int BaseXP { get; set; }
    public int MinLevel { get; set; } = 1;
    public int MaxLevel { get; set; } = 99;
    public LifeQuestAPI.Domain.Enums.DifficultyLevel Difficulty { get; set; } = LifeQuestAPI.Domain.Enums.DifficultyLevel.Easy;
    public bool IsDaily { get; set; }
    public int? EstimatedMinutes { get; set; }
    public string? WhyThisTask { get; set; }
}

public class BadgeSeedDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string IconPath { get; set; } = string.Empty;
    public int RequiredLevel { get; set; }
}

public class AchievementSeedDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string IconPath { get; set; } = string.Empty;
    public LifeQuestAPI.Domain.Enums.AchievementType Type { get; set; }
    public int RequiredCount { get; set; }
    public int BonusXP { get; set; }
    public bool IsHidden { get; set; }
}
