using LifeQuestAPI.Application.Abstractions;
using LifeQuestAPI.Application.DTOs;
using LifeQuestAPI.Application.Repositories;
using LifeQuestAPI.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LifeQuestAPI.Application.Features.UserTasks.Queries.GetDailyTaskPool;

public class GetDailyTaskPoolQueryRequest : IRequest<DailyTaskPoolDto>
{
    public Guid UserId { get; set; }
    public int Count { get; set; } = 6;
}

public class GetDailyTaskPoolQueryHandler : IRequestHandler<GetDailyTaskPoolQueryRequest, DailyTaskPoolDto>
{
    private readonly IUserDailyMoodReadRepository _moodReadRepository;
    private readonly IUserTaskReadRepository _userTaskReadRepository;
    private readonly ITaskRecommendationService _recommendationService;

    public GetDailyTaskPoolQueryHandler(
        IUserDailyMoodReadRepository moodReadRepository,
        IUserTaskReadRepository userTaskReadRepository,
        ITaskRecommendationService recommendationService)
    {
        _moodReadRepository = moodReadRepository;
        _userTaskReadRepository = userTaskReadRepository;
        _recommendationService = recommendationService;
    }

    public async Task<DailyTaskPoolDto> Handle(GetDailyTaskPoolQueryRequest request, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // Bugün zaten önerilmiş görevler varsa onları döndür
        var existingSuggested = await _userTaskReadRepository
            .GetWhere(ut => ut.AppUserId == request.UserId
                && (ut.Status == AppTaskStatus.Suggested || ut.Status == AppTaskStatus.Selected)
                && ut.DueDate.HasValue
                && DateOnly.FromDateTime(ut.DueDate.Value) == today, tracking: false)
            .Include(ut => ut.AppTask)
                .ThenInclude(t => t.Category)
            .ToListAsync(cancellationToken);

        // Bugünkü mood kaydını çek (yoksa default)
        var todayMood = await _moodReadRepository
            .GetWhere(m => m.AppUserId == request.UserId && m.Date == today, tracking: false)
            .FirstOrDefaultAsync(cancellationToken);

        var moodMode = todayMood?.MoodMode ?? MoodMode.Normal;
        var availableMinutes = todayMood?.AvailableMinutes ?? 30;
        var energyLevel = todayMood?.EnergyLevel ?? 3;
        var timeSlot = DayTimeSlotExtensions.FromHour(DateTime.UtcNow.Hour);

        if (existingSuggested.Count >= request.Count)
        {
            return new DailyTaskPoolDto
            {
                Tasks = existingSuggested.Select(ut => new TaskRecommendationDto
                {
                    UserTaskId = ut.Id,
                    AppTaskId = ut.AppTaskId,
                    Title = ut.AppTask.Title,
                    Description = ut.AppTask.Description,
                    CategoryName = ut.AppTask.Category?.Name ?? string.Empty,
                    EstimatedMinutes = ut.AppTask.EstimatedMinutes,
                    BaseXP = ut.AppTask.BaseXP,
                    Difficulty = ut.AppTask.Difficulty.ToString(),
                    WhyThisTask = ut.AppTask.WhyThisTask,
                    BadgeHint = ut.AppTask.BadgeHint,
                    MatchScore = 0.8,
                    RecommendationReason = "Bugün için daha önce önerildi",
                    Status = ut.Status.ToString()
                }).ToList(),
                TodayMood = moodMode,
                AvailableMinutes = availableMinutes,
                EnergyLevel = energyLevel,
                TimeSlot = timeSlot,
                PoolGeneratedAt = DateTime.UtcNow
            };
        }

        // Yeni öneri oluştur
        var recommendations = await _recommendationService.GetContextualRecommendationsAsync(
            request.UserId, moodMode, timeSlot, energyLevel, availableMinutes, request.Count);

        return new DailyTaskPoolDto
        {
            Tasks = recommendations,
            TodayMood = moodMode,
            AvailableMinutes = availableMinutes,
            EnergyLevel = energyLevel,
            TimeSlot = timeSlot,
            PoolGeneratedAt = DateTime.UtcNow
        };
    }
}
