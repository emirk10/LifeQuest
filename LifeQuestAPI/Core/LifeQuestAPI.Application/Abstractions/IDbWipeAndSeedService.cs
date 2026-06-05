using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LifeQuestAPI.Application.Abstractions;

public interface IDbWipeAndSeedService
{
    Task<WipeAndSeedServiceResponseDto> WipeAndSeedAsync(WipeAndSeedServiceRequestDto request);
}

public class WipeAndSeedServiceResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class WipeAndSeedServiceRequestDto
{
    public bool WipeDatabase { get; set; } = false;
    public List<CategorySeedRequestDto>? Categories { get; set; }
    public List<TaskSeedRequestDto>? Tasks { get; set; }
    public List<BadgeSeedRequestDto>? Badges { get; set; }
    public List<AchievementSeedRequestDto>? Achievements { get; set; }
}

public class CategorySeedRequestDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? IconPath { get; set; }
}

public class TaskSeedRequestDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public int BaseXP { get; set; }
    public int MinLevel { get; set; } = 1;
    public int MaxLevel { get; set; } = 99;
    public int Difficulty { get; set; }
    public bool IsDaily { get; set; }
    public int? EstimatedMinutes { get; set; }
    public string? WhyThisTask { get; set; }
}

public class BadgeSeedRequestDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string IconPath { get; set; } = string.Empty;
    public int RequiredLevel { get; set; }
}

public class AchievementSeedRequestDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string IconPath { get; set; } = string.Empty;
    public int Type { get; set; }
    public int RequiredCount { get; set; }
    public int BonusXP { get; set; }
    public bool IsHidden { get; set; }
}
