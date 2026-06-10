using LifeQuestAPI.Application.Abstractions;
using LifeQuestAPI.Domain.Entities;
using LifeQuestAPI.Domain.Enums;
using LifeQuestAPI.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LifeQuestAPI.Persistence.Services;

public class DbWipeAndSeedService : IDbWipeAndSeedService
{
    private readonly LifeQuestDbContext _context;

    public DbWipeAndSeedService(LifeQuestDbContext context)
    {
        _context = context;
    }

    public async Task<WipeAndSeedServiceResponseDto> WipeAndSeedAsync(WipeAndSeedServiceRequestDto request)
    {
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
                        
                        var existingCats = await _context.Categories.ToListAsync();
                        foreach (var cat in existingCats)
                        {
                            categoryMapping[cat.Name.ToLower()] = cat;
                        }
                    }

                    
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
                                    Description = cat.Description ?? string.Empty,
                                    IconPath = cat.IconPath ?? string.Empty
                                };
                                _context.Categories.Add(newCat);
                                categoryMapping[catKey] = newCat;
                            }
                        }
                        await _context.SaveChangesAsync();
                    }

                    
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
                                Difficulty = (DifficultyLevel)t.Difficulty,
                                IsDaily = t.IsDaily,
                                IsActive = true,
                                EstimatedMinutes = t.EstimatedMinutes,
                                WhyThisTask = t.WhyThisTask
                            };

                            _context.AppTasks.Add(newAppTask);
                        }
                        await _context.SaveChangesAsync();
                    }

                    
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
                                RequiredValue = b.RequiredLevel
                            });
                        }
                        await _context.SaveChangesAsync();
                    }

                    
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
                                Type = (AchievementType)a.Type,
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

                    return new WipeAndSeedServiceResponseDto { Success = true, Message = msg };
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return new WipeAndSeedServiceResponseDto { Success = false, Message = "Seeding hatası: " + ex.Message };
                }
            });
        }
        catch (Exception ex)
        {
            return new WipeAndSeedServiceResponseDto { Success = false, Message = "Database execution hatası: " + ex.Message };
        }
    }
}
