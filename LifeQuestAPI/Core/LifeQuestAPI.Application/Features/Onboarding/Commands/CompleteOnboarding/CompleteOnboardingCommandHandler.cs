using LifeQuestAPI.Application.Abstractions;
using LifeQuestAPI.Application.Repositories;
using LifeQuestAPI.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace LifeQuestAPI.Application.Features.Onboarding.Commands.CompleteOnboarding;

public class CompleteOnboardingCommandRequest : IRequest<CompleteOnboardingCommandResponse>
{
    public Guid UserId { get; set; }

    // Adım 1 — Karşılama
    public string? DisplayName { get; set; }
    public int? AvatarId { get; set; }

    // Adım 2 — Yaşam tarzı
    public string? LifestyleType { get; set; }
    public string? WakeUpTime { get; set; }  // "07:00"
    public List<int>? WorkDays { get; set; }

    // Adım 3 — Hedefler
    public string? PrimaryFocusArea { get; set; }
    public List<string>? SecondaryGoals { get; set; }

    // Adım 4 — Kapasite
    public int? DailyAvailableMinutes { get; set; }
    public string? PreferredTaskDuration { get; set; }
    public int? PreferredTasksPerWeek { get; set; }

    // Adım 5 — Motivasyon
    public string? MotivationFactor { get; set; }
    public List<string>? MotivationKillers { get; set; }

    // Adım 6 — Zorluk
    public string? DifficultyPreference { get; set; }
    public string? AgeRange { get; set; }
}

public class CompleteOnboardingCommandResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public Guid? FirstTaskId { get; set; }
    public string? FirstTaskTitle { get; set; }
    public int DailyTaskLimit { get; set; }
    public string? PrimaryFocusArea { get; set; }
    public int? AvatarId { get; set; }
    public string? DisplayName { get; set; }
}

public sealed class CompleteOnboardingCommandHandler
    : IRequestHandler<CompleteOnboardingCommandRequest, CompleteOnboardingCommandResponse>
{
    private readonly IAppUserReadRepository _userReadRepository;
    private readonly IAppUserWriteRepository _userWriteRepository;
    private readonly IOnboardingResponseWriteRepository _onboardingWriteRepository;
    private readonly ITaskRecommendationService _taskRecommendationService;

    public CompleteOnboardingCommandHandler(
        IAppUserReadRepository userReadRepository,
        IAppUserWriteRepository userWriteRepository,
        IOnboardingResponseWriteRepository onboardingWriteRepository,
        ITaskRecommendationService taskRecommendationService)
    {
        _userReadRepository = userReadRepository;
        _userWriteRepository = userWriteRepository;
        _onboardingWriteRepository = onboardingWriteRepository;
        _taskRecommendationService = taskRecommendationService;
    }

    public async Task<CompleteOnboardingCommandResponse> Handle(
        CompleteOnboardingCommandRequest request, CancellationToken cancellationToken)
    {
        var user = await _userReadRepository.GetByIdAsync(request.UserId.ToString(), tracking: true)
            ?? throw new Exception("Kullanıcı bulunamadı.");

        // AppUser alanlarını güncelle
        if (!string.IsNullOrWhiteSpace(request.DisplayName))
            user.Name = request.DisplayName;

        user.AvatarId = request.AvatarId;
        user.LifestyleType = request.LifestyleType;
        user.AgeRange = request.AgeRange;
        user.PrimaryFocusArea = request.PrimaryFocusArea;
        user.PreferredTaskDuration = request.PreferredTaskDuration;
        user.DifficultyPreference = request.DifficultyPreference;
        user.MotivationFactor = request.MotivationFactor;
        user.MotivationKillers = request.MotivationKillers;
        user.WorkDays = request.WorkDays;

        if (!string.IsNullOrWhiteSpace(request.WakeUpTime) &&
            TimeOnly.TryParse(request.WakeUpTime, out var wakeTime))
            user.WakeUpTime = wakeTime;

        if (request.DailyAvailableMinutes.HasValue)
            user.DailyAvailableMinutes = request.DailyAvailableMinutes;

        if (request.PreferredTasksPerWeek.HasValue)
        {
            user.PreferredTasksPerWeek = request.PreferredTasksPerWeek;
            user.WeeklyTaskLimit = request.PreferredTasksPerWeek.Value;
            // DailyTaskLimit = max(1, haftada / 7)
            user.DailyTaskLimit = Math.Max(1, request.PreferredTasksPerWeek.Value / 7);
        }

        user.OnboardingCompleted = true;
        user.OnboardingCompletedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;

        _userWriteRepository.Update(user);
        await _userWriteRepository.SaveAsync();

        // Onboarding ham cevaplarını kaydet
        var answers = new Dictionary<string, object?>
        {
            ["display_name"] = request.DisplayName,
            ["avatar_id"] = request.AvatarId,
            ["lifestyle_type"] = request.LifestyleType,
            ["wake_up_time"] = request.WakeUpTime,
            ["work_days"] = request.WorkDays,
            ["primary_focus_area"] = request.PrimaryFocusArea,
            ["secondary_goals"] = request.SecondaryGoals,
            ["daily_available_minutes"] = request.DailyAvailableMinutes,
            ["preferred_task_duration"] = request.PreferredTaskDuration,
            ["preferred_tasks_per_week"] = request.PreferredTasksPerWeek,
            ["motivation_factor"] = request.MotivationFactor,
            ["motivation_killers"] = request.MotivationKillers,
            ["difficulty_preference"] = request.DifficultyPreference,
            ["age_range"] = request.AgeRange,
        };

        foreach (var (key, value) in answers)
        {
            if (value == null) continue;
            await _onboardingWriteRepository.AddAsync(new OnboardingResponse
            {
                AppUserId = request.UserId,
                QuestionKey = key,
                Answer = JsonSerializer.Serialize(value),
                AnsweredAt = DateTime.UtcNow
            });
        }
        await _onboardingWriteRepository.SaveAsync();

        // İlk görev önerisi al (1 adet, Easy, PrimaryFocusArea)
        Guid? firstTaskId = null;
        string? firstTaskTitle = null;

        try
        {
            var recommendations = await _taskRecommendationService.GetContextualRecommendationsAsync(
                request.UserId,
                Domain.Enums.MoodMode.Normal,
                Domain.Enums.DayTimeSlot.Morning,
                energyLevel: 3,
                availableMinutes: request.DailyAvailableMinutes ?? 30,
                count: 1);

            if (recommendations.Count > 0)
            {
                firstTaskId = recommendations[0].UserTaskId;
                firstTaskTitle = recommendations[0].Title;
            }
        }
        catch { /* İlk görev alınamazsa sessizce geç */ }

        return new CompleteOnboardingCommandResponse
        {
            Success = true,
            Message = "Onboarding tamamlandı! Hoş geldin! 🎉",
            FirstTaskId = firstTaskId,
            FirstTaskTitle = firstTaskTitle,
            DailyTaskLimit = user.DailyTaskLimit,
            PrimaryFocusArea = user.PrimaryFocusArea,
            AvatarId = user.AvatarId,
            DisplayName = user.Name
        };
    }
}
