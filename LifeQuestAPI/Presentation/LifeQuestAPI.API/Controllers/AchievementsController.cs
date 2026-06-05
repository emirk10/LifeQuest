using LifeQuestAPI.Application.Repositories;
using LifeQuestAPI.Domain.Entities;
using LifeQuestAPI.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace LifeQuestAPI.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class AchievementsController : ControllerBase
{
    private readonly IAchievementReadRepository _achievementRead;
    private readonly IAchievementWriteRepository _achievementWrite;

    public AchievementsController(
        IAchievementReadRepository achievementRead,
        IAchievementWriteRepository achievementWrite)
    {
        _achievementRead = achievementRead;
        _achievementWrite = achievementWrite;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var achievements = await _achievementRead.GetAll(false).ToListAsync();
        return Ok(new { success = true, achievements });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById([FromRoute] Guid id)
    {
        var achievement = await _achievementRead.GetByIdAsync(id.ToString(), tracking: false);
        if (achievement == null) return NotFound(new { success = false, message = "Başarım bulunamadı." });
        return Ok(new { success = true, achievement });
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateAchievementDto dto)
    {
        var achievement = new Achievement
        {
            Title = dto.Title.Trim(),
            Description = dto.Description.Trim(),
            IconPath = dto.IconPath.Trim(),
            Type = dto.Type,
            RequiredCount = dto.RequiredCount,
            BonusXP = dto.BonusXP,
            IsHidden = dto.IsHidden
        };

        await _achievementWrite.AddAsync(achievement);
        await _achievementWrite.SaveAsync();

        return Ok(new { success = true, message = "Başarım oluşturuldu.", achievementId = achievement.Id });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] CreateAchievementDto dto)
    {
        var achievement = await _achievementRead.GetByIdAsync(id.ToString(), tracking: true);
        if (achievement == null) return NotFound(new { success = false, message = "Başarım bulunamadı." });

        achievement.Title = dto.Title.Trim();
        achievement.Description = dto.Description.Trim();
        achievement.IconPath = dto.IconPath.Trim();
        achievement.Type = dto.Type;
        achievement.RequiredCount = dto.RequiredCount;
        achievement.BonusXP = dto.BonusXP;
        achievement.IsHidden = dto.IsHidden;

        _achievementWrite.Update(achievement);
        await _achievementWrite.SaveAsync();

        return Ok(new { success = true, message = "Başarım güncellendi." });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        var achievement = await _achievementRead.GetByIdAsync(id.ToString(), tracking: true);
        if (achievement == null) return NotFound(new { success = false, message = "Başarım bulunamadı." });

        _achievementWrite.Remove(achievement);
        await _achievementWrite.SaveAsync();

        return Ok(new { success = true, message = "Başarım silindi." });
    }
}

public class CreateAchievementDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string IconPath { get; set; } = string.Empty;
    public AchievementType Type { get; set; }
    public int RequiredCount { get; set; }
    public int BonusXP { get; set; }
    public bool IsHidden { get; set; }
}
