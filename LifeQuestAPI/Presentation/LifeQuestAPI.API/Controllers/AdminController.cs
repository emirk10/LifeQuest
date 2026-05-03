using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LifeQuestAPI.Persistence.Contexts;
using LifeQuestAPI.Domain.Entities;
using LifeQuestAPI.Domain.Enums;
using System.Text.Json;

namespace LifeQuestAPI.API.Controllers;

[Route("api/admin")]
[ApiController]
[Authorize]
public class AdminController : ControllerBase
{
    private readonly LifeQuestDbContext _context;

    public AdminController(LifeQuestDbContext context)
    {
        _context = context;
    }

    [HttpPost("wipe-and-seed")]
    public async Task<IActionResult> WipeAndSeed([FromBody] WipeAndSeedRequest request)
    {
        if (request == null || (request.Categories == null && request.Badges == null && request.Achievements == null))
            return BadRequest(new { success = false, message = "Geçersiz veya boş veri!" });

        var strategy = _context.Database.CreateExecutionStrategy();
        try
        {
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var categoryMapping = new Dictionary<string, Category>();

                    if (request.WipeDatabase)
                    {
                        // 1. Wipe everything safely
                        _context.UserCategoryProgresses.RemoveRange(_context.UserCategoryProgresses);
                        _context.UserTasks.RemoveRange(_context.UserTasks);
                        _context.AppTasks.RemoveRange(_context.AppTasks);
                        _context.Categories.RemoveRange(_context.Categories);
                        _context.UserBadges.RemoveRange(_context.UserBadges);
                        _context.Badges.RemoveRange(_context.Badges);
                        _context.UserAchievements.RemoveRange(_context.UserAchievements);
                        _context.Achievements.RemoveRange(_context.Achievements);

                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        // Load existing categories to avoid duplicates
                        var existingCats = await _context.Categories.ToListAsync();
                        foreach (var cat in existingCats)
                        {
                            categoryMapping[cat.Name.ToLower()] = cat;
                        }
                    }

                    // 2. Insert Categories and map to their IDs
                    if (request.Categories != null)
                    {
                        foreach (var cat in request.Categories)
                        {
                            var catKey = cat.Name.ToLower();
                            if (!categoryMapping.ContainsKey(catKey))
                            {
                                var newCat = new Category
                                {
                                    Id = Guid.NewGuid(),
                                    Name = cat.Name,
                                    Description = cat.Description,
                                    IconPath = cat.IconPath
                                };
                                _context.Categories.Add(newCat);
                                categoryMapping[catKey] = newCat;
                            }
                        }
                        await _context.SaveChangesAsync();
                    }

                    // 3. Insert Tasks
                    if (request.Tasks != null)
                    {
                        foreach (var t in request.Tasks)
                        {
                            var catKey = (t.CategoryName ?? "genel").ToLower();
                            var category = categoryMapping.ContainsKey(catKey) 
                                ? categoryMapping[catKey] 
                                : (categoryMapping.Values.FirstOrDefault() ?? new Category { Id = Guid.NewGuid(), Name = "Genel", IconPath = "🎯" });

                            if (category.Id == Guid.Empty || !_context.Categories.Any(c => c.Id == category.Id))
                            {
                                _context.Categories.Add(category);
                                await _context.SaveChangesAsync();
                                categoryMapping[category.Name.ToLower()] = category;
                            }

                            var newAppTask = new AppTask
                            {
                                Id = Guid.NewGuid(),
                                Title = t.Title,
                                Description = t.Description,
                                CategoryId = category.Id,
                                BaseXP = t.BaseXP,
                                MinLevel = t.MinLevel,
                                MaxLevel = t.MaxLevel,
                                Difficulty = t.Difficulty,
                                IsDaily = t.IsDaily,
                                IsActive = true,
                                EstimatedMinutes = t.EstimatedMinutes,
                                WhyThisTask = t.WhyThisTask
                            };

                            _context.AppTasks.Add(newAppTask);
                        }
                        await _context.SaveChangesAsync();
                    }

                    // 4. Insert Badges
                    if (request.Badges != null)
                    {
                        foreach (var b in request.Badges)
                        {
                            _context.Badges.Add(new Badge
                            {
                                Id = Guid.NewGuid(),
                                Name = b.Name,
                                Description = b.Description,
                                IconPath = b.IconPath,
                                RequiredLevel = b.RequiredLevel
                            });
                        }
                        await _context.SaveChangesAsync();
                    }

                    // 5. Insert Achievements
                    if (request.Achievements != null)
                    {
                        foreach (var a in request.Achievements)
                        {
                            _context.Achievements.Add(new Achievement
                            {
                                Id = Guid.NewGuid(),
                                Title = a.Title,
                                Description = a.Description,
                                IconPath = a.IconPath,
                                Type = a.Type,
                                RequiredCount = a.RequiredCount,
                                BonusXP = a.BonusXP,
                                IsHidden = a.IsHidden
                            });
                        }
                        await _context.SaveChangesAsync();
                    }

                    await transaction.CommitAsync();

                    var msg = request.WipeDatabase 
                        ? "DB tamamen sıfırlandı ve yeni JSON verileriyle tekrar dolduruldu!" 
                        : "Mevcut veriler korundu ve yeni JSON verileri başarıyla eklendi!";

                    return Ok(new { success = true, message = msg });
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = "Seeding hatası: " + ex.Message });
        }
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
    public DifficultyLevel Difficulty { get; set; } = DifficultyLevel.Easy;
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
    public AchievementType Type { get; set; }
    public int RequiredCount { get; set; }
    public int BonusXP { get; set; }
    public bool IsHidden { get; set; }
}
