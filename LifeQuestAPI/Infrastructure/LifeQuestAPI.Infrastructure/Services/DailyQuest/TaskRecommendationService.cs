using LifeQuestAPI.Application.Abstractions;
using LifeQuestAPI.Application.DTOs;
using LifeQuestAPI.Application.Repositories;
using LifeQuestAPI.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace LifeQuestAPI.Infrastructure.Services.DailyQuest;

/// <summary>
/// SP-01 + SP-03: Bağlam-duyarlı görev öneri servisi.
/// SP-01: 6 faktörlü scoring: GoalMatch, TimeMatch, EnergyMatch, DifficultyMatch, WeaknessTarget, StrengthMatch
/// SP-03: +3 faktör: BehaviorBonus, MostSkippedPenalty, TimeOfDayBonus
/// </summary>
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
        // Mood override: Overwhelmed ise max 1 görev
        if (mood == MoodMode.Overwhelmed) count = 1;

        // Kullanıcı profilini al
        var user = await _userReadRepository
            .GetWhere(u => u.Id == userId, tracking: false)
            .FirstOrDefaultAsync();

        if (user == null) throw new Exception("Kullanıcı bulunamadı.");

        // Bugün zaten görev atanmış AppTaskId'leri bul (hariç tut)
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var assignedTaskIds = await _userTaskReadRepository
            .GetWhere(ut => ut.AppUserId == userId
                && ut.Status != AppTaskStatus.Expired
                && ut.Status != AppTaskStatus.Abandoned
                && (ut.DueDate == null || DateOnly.FromDateTime(ut.DueDate.Value) == today),
                tracking: false)
            .Select(ut => ut.AppTaskId)
            .ToListAsync();

        // Uygun görevleri çek
        var candidateTasks = await _taskReadRepository
            .GetWhere(t => t.IsActive
                && !assignedTaskIds.Contains(t.Id)
                && t.MinLevel <= (int)Math.Floor(user.GeneralLevel),
                tracking: false)
            .Include(t => t.Category)
            .ToListAsync();

        // Eğer veritabanında veya aktif havuzda görev kalmadıysa
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

        // Her görev için puan hesapla
        var scored = candidateTasks
            .Select(task => new
            {
                Task = task,
                Score = CalculateScore(task, user, mood, timeSlot, energyLevel, availableMinutes),
                Reason = BuildReason(task, user, mood, timeSlot, energyLevel)
            })
            .Where(x => x.Score >= 0) // skoru 0 olanları da alsın eğer görev kalmadıysa
            .OrderByDescending(x => x.Score)
            .Take(count)
            .ToList();

        // UserTask olarak kaydet (Status=Suggested)
        var result = new List<TaskRecommendationDto>();
        foreach (var item in scored)
        {
            var userTask = new Domain.Entities.UserTask
            {
                AppUserId = userId,
                AppTaskId = item.Task.Id,
                Status = AppTaskStatus.Suggested,
                DayTimeSlot = timeSlot,
                DueDate = DateTime.UtcNow.Date.AddDays(1).AddTicks(-1) // gün sonu
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
        int availableMinutes)
    {
        double score = 0.0;

        // 1. GoalMatch (0.25) — kullanıcının hedefleriyle görev kategorisi eşleşiyor mu?
        var userGoals = user.UserGoals ?? new List<string>();
        var taskGoals = task.TargetedGoals ?? new List<string>();
        if (userGoals.Any() && (userGoals.Contains(task.Category?.Name?.ToLower() ?? "") || taskGoals.Any(g => userGoals.Contains(g))))
            score += 0.25;

        // 2. TimeMatch (0.20) — görev müsait süreye sığıyor mu?
        bool timeSlotMatch = task.RecommendedTimeSlots == null || !task.RecommendedTimeSlots.Any()
            || task.RecommendedTimeSlots.Contains(timeSlot);
        bool durationFits = !task.EstimatedMinutes.HasValue || task.EstimatedMinutes.Value <= availableMinutes;
        if (timeSlotMatch && durationFits) score += 0.20;

        // 3. EnergyMatch (0.20) — enerji seviyesi görevin gereksinimlerine uyuyor mu?
        if (energyLevel >= task.MinEnergyLevel && energyLevel <= task.MaxEnergyLevel)
            score += 0.20;

        // 4. DifficultyMatch (0.15)
        var diffPref = user.DifficultyPreference?.ToLower() ?? "mixed";
        bool diffMatch = diffPref == "mixed" || task.Difficulty.ToString().ToLower() == diffPref;
        if (diffMatch) score += 0.15;

        // 5. WeaknessTarget (0.10) — görev kullanıcının zayıflıklarını hedefliyor mu?
        var userWeaknesses = user.Weaknesses ?? new List<string>();
        var taskWeaknesses = task.TargetedWeaknesses ?? new List<string>();
        if (userWeaknesses.Any() && taskWeaknesses.Any(w => userWeaknesses.Contains(w)))
            score += 0.10;

        // 6. StrengthMatch (0.10)
        var userStrengths = user.Strengths ?? new List<string>();
        if (userStrengths.Any() && userStrengths.Any(s =>
            (task.Description?.Contains(s, StringComparison.OrdinalIgnoreCase) == true) ||
            (task.Title?.Contains(s, StringComparison.OrdinalIgnoreCase) == true)))
            score += 0.10;

        // ── SP-03: Davranış Faktörleri ──────────────────────────────────

        // 7. BehaviorBonus (0.10) — Bu kategoride geçmişte başarılıysa bonus
        //    user.MostSkippedCategory dolu ve bu görev o kategoride değilse bonus
        if (!string.IsNullOrEmpty(user.MostSkippedCategory))
        {
            bool isMostSkipped = string.Equals(
                task.Category?.Name, user.MostSkippedCategory,
                StringComparison.OrdinalIgnoreCase);

            // 8. MostSkippedPenalty (-0.15)
            if (isMostSkipped)
                score -= 0.15;
            else
                score += 0.10; // BehaviorBonus — farklı kategori başarı şansı daha yüksek
        }

        // 9. TimeOfDayBonus (0.05) — Kullanıcının en verimli saat dilimiyle eşleşiyorsa bonus
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

        // Mood override bonus'lar
        if (mood == MoodMode.Energetic && task.MinEnergyLevel >= 4)
            score += 0.10;
        if (mood == MoodMode.LimitedTime && task.EstimatedMinutes <= 15)
            score += 0.10;
        if (mood == MoodMode.Tired && task.MaxEnergyLevel <= 2)
            score += 0.05;
        if ((mood == MoodMode.Tired || mood == MoodMode.Overwhelmed) && task.MinEnergyLevel > 3)
            score -= 0.20; // Yorgunken zorlu görevleri alt sıralara it

        return Math.Max(0, score);
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
        if (userGoals.Any(g => task.Category?.Name?.ToLower().Contains(g) == true))
            parts.Add($"{task.Category?.Name} hedefine uygun");

        if (task.EstimatedMinutes.HasValue)
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
