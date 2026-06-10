using LifeQuestAPI.Application.Abstractions;
using LifeQuestAPI.Application.DTOs;
using LifeQuestAPI.Application.Repositories;
using LifeQuestAPI.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace LifeQuestAPI.Infrastructure.Services.DailyQuest;






public sealed class TaskRecommendationService : ITaskRecommendationService
{
    private readonly IAppUserReadRepository _userReadRepository;
    private readonly IAppTaskReadRepository _taskReadRepository;
    private readonly IUserTaskReadRepository _userTaskReadRepository;
    private readonly IUserTaskWriteRepository _userTaskWriteRepository;

    public TaskRecommendationService(
        IAppUserReadRepository userReadRepository,
        IAppTaskReadRepository taskReadRepository,
        IUserTaskReadRepository userTaskReadRepository,
        IUserTaskWriteRepository userTaskWriteRepository)
    {
        _userReadRepository = userReadRepository;
        _taskReadRepository = taskReadRepository;
        _userTaskReadRepository = userTaskReadRepository;
        _userTaskWriteRepository = userTaskWriteRepository;
    }

    public async Task<List<TaskRecommendationDto>> GetContextualRecommendationsAsync(
        Guid userId,
        MoodMode mood,
        DayTimeSlot timeSlot,
        int energyLevel,
        int availableMinutes,
        int count = 6)
    {
        
        if (mood == MoodMode.Overwhelmed) count = 1;

        
        var user = await _userReadRepository
            .GetWhere(u => u.Id == userId, tracking: false)
            .FirstOrDefaultAsync();

        if (user == null) throw new Exception("Kullanıcı bulunamadı.");

        
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);

        var userTaskHistory = await _userTaskReadRepository
            .GetWhere(ut => ut.AppUserId == userId && ut.CreatedAt >= sevenDaysAgo, tracking: false)
            .ToListAsync();

        var assignedTaskIds = userTaskHistory
            .Where(ut => ut.Status != AppTaskStatus.Expired
                && ut.Status != AppTaskStatus.Abandoned
                && (ut.DueDate == null || DateOnly.FromDateTime(ut.DueDate.Value) == today))
            .Select(ut => ut.AppTaskId)
            .ToList();

        var abandonedTaskIds = userTaskHistory
            .Where(ut => ut.Status == AppTaskStatus.Abandoned)
            .Select(ut => ut.AppTaskId)
            .ToList();

        var recentlyCompletedTaskIds = userTaskHistory
            .Where(ut => ut.Status == AppTaskStatus.Completed)
            .Select(ut => ut.AppTaskId)
            .ToList();

        
        var candidateTasks = await _taskReadRepository
            .GetWhere(t => t.IsActive
                && !assignedTaskIds.Contains(t.Id)
                && t.MinLevel <= (int)Math.Floor(user.GeneralLevel),
                tracking: false)
            .Include(t => t.Category)
            .ToListAsync();

        
        if (!candidateTasks.Any())
        {
            candidateTasks = await _taskReadRepository
                .GetWhere(t => t.IsActive && t.MinLevel <= (int)Math.Floor(user.GeneralLevel), tracking: false)
                .Include(t => t.Category)
                .ToListAsync();

            if (!candidateTasks.Any())
            {
                var defaultCategory = await _taskReadRepository.Table.Select(t => t.Category).FirstOrDefaultAsync();
                candidateTasks = new List<Domain.Entities.AppTask>
                {
                    new() { Title = "10 Dakikalık Meditasyon", Description = "Zihnini dinlendir ve odaklan.", BaseXP = 50, IsActive = true, Category = defaultCategory, MinLevel = 1, WhyThisTask = "Zihinsel sağlık için.", EstimatedMinutes = 10, MinEnergyLevel = 1, MaxEnergyLevel = 5 },
                    new() { Title = "Kısa Bir Yürüyüş", Description = "Açık havada 15 dakika yürüyüş yap.", BaseXP = 60, IsActive = true, Category = defaultCategory, MinLevel = 1, WhyThisTask = "Fiziksel zindelik için.", EstimatedMinutes = 15, MinEnergyLevel = 1, MaxEnergyLevel = 5 },
                    new() { Title = "Kitap Oku", Description = "Gelişimin için en az 10 sayfa oku.", BaseXP = 75, IsActive = true, Category = defaultCategory, MinLevel = 1, WhyThisTask = "Kişisel gelişim için.", EstimatedMinutes = 20, MinEnergyLevel = 1, MaxEnergyLevel = 5 }
                };
            }
        }

        
        var scoredList = candidateTasks
            .Select(task => new
            {
                Task = task,
                Score = CalculateScore(task, user, mood, timeSlot, energyLevel, availableMinutes, abandonedTaskIds, recentlyCompletedTaskIds),
                Reason = BuildReason(task, user, mood, timeSlot, energyLevel)
            })
            .Where(x => x.Score > 0.0) 
            .OrderByDescending(x => x.Score)
            .ToList();

        
        if (scoredList.Count < count)
        {
            var defaultCategory = await _taskReadRepository.Table.Select(t => t.Category).FirstOrDefaultAsync();
            var fallbackTasks = new List<Domain.Entities.AppTask>
            {
                new() { Id = Guid.NewGuid(), Title = "10 Dakikalık Meditasyon", Description = "Zihnini dinlendir ve odaklan.", BaseXP = 50, IsActive = true, Category = defaultCategory, MinLevel = 1, WhyThisTask = "Zihinsel sağlık için.", EstimatedMinutes = 10, MinEnergyLevel = 1, MaxEnergyLevel = 5 },
                new() { Id = Guid.NewGuid(), Title = "Kısa Bir Yürüyüş", Description = "Açık havada 15 dakika yürüyüş yap.", BaseXP = 60, IsActive = true, Category = defaultCategory, MinLevel = 1, WhyThisTask = "Fiziksel zindelik için.", EstimatedMinutes = 15, MinEnergyLevel = 1, MaxEnergyLevel = 5 },
                new() { Id = Guid.NewGuid(), Title = "Kitap Oku", Description = "Gelişimin için en az 10 sayfa oku.", BaseXP = 75, IsActive = true, Category = defaultCategory, MinLevel = 1, WhyThisTask = "Kişisel gelişim için.", EstimatedMinutes = 20, MinEnergyLevel = 1, MaxEnergyLevel = 5 }
            };

            foreach (var fb in fallbackTasks)
            {
                if (scoredList.Count >= count) break;
                if (scoredList.Any(s => s.Task.Title == fb.Title)) continue;

                scoredList.Add(new
                {
                    Task = fb,
                    Score = 0.1,
                    Reason = "Genel zindelik ve gelişim önerisi"
                });
            }
        }

        var scored = scoredList.Take(count).ToList();

        
        var result = new List<TaskRecommendationDto>();
        var selectedCategories = new Dictionary<Guid, int>();

        foreach (var item in scored)
        {
            
            
            
            var catId = item.Task.CategoryId;
            selectedCategories.TryGetValue(catId, out int catCount);
            if (catCount >= 2) continue; 
            
            selectedCategories[catId] = catCount + 1;

            var userTask = new Domain.Entities.UserTask
            {
                AppUserId = userId,
                AppTaskId = item.Task.Id,
                Status = AppTaskStatus.Suggested,
                DayTimeSlot = timeSlot,
                DueDate = DateTime.UtcNow.Date.AddDays(1).AddTicks(-1) 
            };
            await _userTaskWriteRepository.AddAsync(userTask);

            result.Add(new TaskRecommendationDto
            {
                UserTaskId = userTask.Id,
                AppTaskId = item.Task.Id,
                Title = item.Task.Title,
                Description = item.Task.Description,
                CategoryName = item.Task.Category?.Name ?? string.Empty,
                EstimatedMinutes = item.Task.EstimatedMinutes.HasValue && item.Task.EstimatedMinutes.Value > 0 
                    ? item.Task.EstimatedMinutes 
                    : (item.Task.BaseXP > 0 ? (int)Math.Ceiling(item.Task.BaseXP / 5.0) : 15),
                BaseXP = item.Task.BaseXP,
                Difficulty = item.Task.Difficulty.ToString(),
                WhyThisTask = item.Task.WhyThisTask,
                BadgeHint = item.Task.BadgeHint,
                MatchScore = Math.Round(item.Score, 2),
                RecommendationReason = item.Reason,
                Status = "Suggested"
            });
            
            if (result.Count >= count) break;
        }

        await _userTaskWriteRepository.SaveAsync();
        return result;
    }

    private static double CalculateScore(
        Domain.Entities.AppTask task,
        Domain.Entities.Identity.AppUser user,
        MoodMode mood,
        DayTimeSlot timeSlot,
        int energyLevel,
        int availableMinutes,
        List<Guid> abandonedTaskIds,
        List<Guid> recentlyCompletedTaskIds)
    {
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(user);

        
        int estimatedMin = task.EstimatedMinutes ?? (task.BaseXP > 0 ? (int)Math.Ceiling(task.BaseXP / 5.0) : 15);
        if (estimatedMin > availableMinutes) return 0.0; 
        if (energyLevel < task.MinEnergyLevel) return 0.0; 

        
        if (mood == MoodMode.Overwhelmed)
        {
            
            if (task.Difficulty != DifficultyLevel.Easy || task.MinEnergyLevel > 2 || estimatedMin > 15)
                return 0.0;
        }
        else if (mood == MoodMode.Tired)
        {
            
            if (task.MinEnergyLevel >= 4 || task.Difficulty == DifficultyLevel.Hard)
                return 0.0;
        }

        double score = 0.0;

        
        var userGoals = user.UserGoals ?? new List<string>();
        var taskGoals = task.TargetedGoals ?? new List<string>();
        var categoryName = task.Category?.Name?.ToLower() ?? string.Empty;
        if (userGoals.Any() && (userGoals.Contains(categoryName) || taskGoals.Any(g => userGoals.Contains(g))))
            score += 0.25;

        
        bool timeSlotMatch = task.RecommendedTimeSlots == null || !task.RecommendedTimeSlots.Any()
            || task.RecommendedTimeSlots.Contains(timeSlot);
        if (timeSlotMatch) score += 0.20;

        
        if (energyLevel >= task.MinEnergyLevel && energyLevel <= task.MaxEnergyLevel)
            score += 0.20;

        
        var diffPref = user.DifficultyPreference?.ToLower() ?? "mixed";
        bool diffMatch = diffPref == "mixed" || task.Difficulty.ToString().ToLower() == diffPref;
        if (diffMatch) score += 0.15;

        
        var userWeaknesses = user.Weaknesses ?? new List<string>();
        var taskWeaknesses = task.TargetedWeaknesses ?? new List<string>();
        if (userWeaknesses.Any() && taskWeaknesses.Any(w => userWeaknesses.Contains(w)))
            score += 0.10;

        
        var userStrengths = user.Strengths ?? new List<string>();
        var taskDescription = task.Description ?? string.Empty;
        var taskTitle = task.Title ?? string.Empty;
        if (userStrengths.Any() && userStrengths.Any(s =>
            taskDescription.Contains(s, StringComparison.OrdinalIgnoreCase) ||
            taskTitle.Contains(s, StringComparison.OrdinalIgnoreCase)))
            score += 0.10;

        

        
        if (abandonedTaskIds.Contains(task.Id))
            score -= 0.30;

        
        if (recentlyCompletedTaskIds.Contains(task.Id))
            score -= 0.20;

        
        if (user.CurrentStreak > 0)
            score += 0.05;

        
        if (!string.IsNullOrEmpty(user.MostSkippedCategory))
        {
            bool isMostSkipped = string.Equals(
                task.Category?.Name, user.MostSkippedCategory,
                StringComparison.OrdinalIgnoreCase);

            if (isMostSkipped)
                score -= 0.15;
            else
                score += 0.10;
        }

        
        if (user.PreferredMorningTime.HasValue)
        {
            var preferredSlot = user.PreferredMorningTime.Value.Hour switch
            {
                >= 6 and < 11 => DayTimeSlot.Morning,
                >= 11 and < 14 => DayTimeSlot.Noon,
                >= 14 and < 18 => DayTimeSlot.Afternoon,
                >= 18 and < 22 => DayTimeSlot.Evening,
                _ => DayTimeSlot.Night
            };
            bool slotMatch = task.RecommendedTimeSlots == null || !task.RecommendedTimeSlots.Any()
                || task.RecommendedTimeSlots.Contains(preferredSlot);
            if (slotMatch && preferredSlot == timeSlot)
                score += 0.05;
        }

        
        if (mood == MoodMode.Energetic)
        {
            if (task.MinEnergyLevel >= 3)
                score += 0.25; 
            if (task.Difficulty >= DifficultyLevel.Medium)
                score += 0.20; 
        }
        else if (mood == MoodMode.LimitedTime && estimatedMin <= 15)
        {
            score += 0.30; 
        }
        else if (mood == MoodMode.Tired && task.MaxEnergyLevel <= 2)
        {
            score += 0.25; 
        }
        else if (mood == MoodMode.Overwhelmed)
        {
            var catName = task.Category?.Name?.ToLower() ?? string.Empty;
            if (catName.Contains("gelişim") || catName.Contains("sağlık"))
                score += 0.35; 
        }

        return Math.Max(0.01, score); 
    }

    private static string BuildReason(
        Domain.Entities.AppTask task,
        Domain.Entities.Identity.AppUser user,
        MoodMode mood,
        DayTimeSlot timeSlot,
        int energyLevel)
    {
        var parts = new List<string>();

        var userGoals = user.UserGoals ?? new List<string>();
        if (userGoals.Any(g => task.Category?.Name?.ToLower()?.Contains(g) == true))
            parts.Add($"{task.Category?.Name} hedefine uygun");

        if (task.EstimatedMinutes.HasValue && task.EstimatedMinutes.Value > 0)
            parts.Add($"{task.EstimatedMinutes} dk içinde tamamlanabilir");

        var slotLabel = timeSlot switch
        {
            DayTimeSlot.Morning => "sabah verimliliğini artırır",
            DayTimeSlot.Noon => "öğle molasında yapılabilir",
            DayTimeSlot.Afternoon => "öğleden sonra enerjini kullanır",
            DayTimeSlot.Evening => "akşam rutinine uygun",
            DayTimeSlot.Night => "gece sessizliğinde odaklanmana yardımcı olur",
            _ => string.Empty
        };
        if (!string.IsNullOrEmpty(slotLabel)) parts.Add(slotLabel);

        if (mood == MoodMode.LimitedTime) parts.Add("az vaktinle yapılabilir");
        if (mood == MoodMode.Tired && task.MaxEnergyLevel <= 2) parts.Add("yorgunken bile kaldırabilirsin");

        return parts.Count > 0
            ? string.Join(", ", parts)
            : "Senin için önerilen bir görev";
    }
}
