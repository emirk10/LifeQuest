using LifeQuestAPI.Application.Abstractions.Notifications;
using LifeQuestAPI.Application.DTOs;
using LifeQuestAPI.Application.Repositories;
using LifeQuestAPI.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LifeQuestAPI.Application.Features.UserTasks.Commands.SelectTask;

public class SelectTaskCommandRequest : IRequest<UserTaskDto>
{
    public Guid UserId { get; set; }
    public Guid UserTaskId { get; set; }
    /// <summary>SP-05: Kullanıcı limit uyarısını onaylayıp aşmak istiyorsa true.</summary>
    public bool ForceOverLimit { get; set; } = false;
}

public class SelectTaskCommandHandler : IRequestHandler<SelectTaskCommandRequest, UserTaskDto>
{
    private readonly IUserTaskReadRepository _userTaskReadRepository;
    private readonly IUserTaskWriteRepository _userTaskWriteRepository;
    private readonly IUserDailyMoodReadRepository _moodReadRepository;
    private readonly IAppUserReadRepository _userReadRepository;
    private readonly IAppUserWriteRepository _userWriteRepository;
    private readonly IAppNotificationService _notificationService;

    public SelectTaskCommandHandler(
        IUserTaskReadRepository userTaskReadRepository,
        IUserTaskWriteRepository userTaskWriteRepository,
        IUserDailyMoodReadRepository moodReadRepository,
        IAppUserReadRepository userReadRepository,
        IAppUserWriteRepository userWriteRepository,
        IAppNotificationService notificationService)
    {
        _userTaskReadRepository = userTaskReadRepository;
        _userTaskWriteRepository = userTaskWriteRepository;
        _moodReadRepository = moodReadRepository;
        _userReadRepository = userReadRepository;
        _userWriteRepository = userWriteRepository;
        _notificationService = notificationService;
    }

    public async Task<UserTaskDto> Handle(SelectTaskCommandRequest request, CancellationToken cancellationToken)
    {
        // Kullanıcıyı çek
        var user = await _userReadRepository.GetByIdAsync(request.UserId.ToString(), tracking: true)
            ?? throw new Exception("Kullanıcı bulunamadı.");

        // ─── SP-05: Lazy Reset Logic ──────────────────────────────────────
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        
        // Günlük sayaç sıfırlama
        if (!user.LastCounterResetDate.HasValue || user.LastCounterResetDate.Value < today)
        {
            user.TodayTaskCount = 0;
            user.OverLimitWarningShown = false;
            user.LastCounterResetDate = today;
        }

        // Haftalık sayaç sıfırlama (Pazartesi kontrolü)
        var isMonday = DateTime.UtcNow.DayOfWeek == DayOfWeek.Monday;
        if (isMonday && (!user.LastWeeklyResetDate.HasValue || user.LastWeeklyResetDate.Value < today))
        {
            user.WeeklyCompletedCount = 0;
            user.LastWeeklyResetDate = today;
        }

        // ─── SP-05: DailyLimitRule ─────────────────────────────────────────
        if (user.TodayTaskCount >= user.DailyTaskLimit)
        {
            if (!request.ForceOverLimit || user.OverLimitWarningShown)
                throw new Exception($"Bugün için görev limitine ({user.TodayTaskCount}/{user.DailyTaskLimit}) ulaştın. Yeni hakların gece yarısı (00:00) tanımlanacaktır. İstersen limitini ayarlardan değiştirebilirsin.");

            // İlk kez aşıyor, uyarı flag'ini set et
            user.OverLimitWarningShown = true;
        }

        // ─── SP-05: ActiveLimitRule ────────────────────────────────────────
        if (user.ActiveTaskCount >= user.DailyTaskLimit * 2)
            throw new Exception($"Henüz tamamlanmamış çok fazla ({user.ActiveTaskCount}/{user.DailyTaskLimit * 2}) görevin var. Yeni görev alabilmek için önce mevcut görevlerini tamamlamalı veya ertelemelisin.");

        var userTask = await _userTaskReadRepository
            .GetWhere(ut => ut.Id == request.UserTaskId && ut.AppUserId == request.UserId, tracking: true)
            .Include(ut => ut.AppTask)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new Exception("Görev bulunamadı.");

        if (userTask.Status != AppTaskStatus.Suggested)
            throw new Exception("Yalnızca önerilmiş görevler seçilebilir.");

        // Bugünkü mood'dan energy ve mood snapshot al
        var todayMood = await _moodReadRepository
            .GetWhere(m => m.AppUserId == request.UserId && m.Date == today, tracking: false)
            .FirstOrDefaultAsync(cancellationToken);

        userTask.Status = AppTaskStatus.Selected;
        userTask.UserEnergyAtStart = todayMood?.EnergyLevel;
        userTask.UserMoodAtStart = todayMood?.MoodMode;

        // SP-05: Sayaçları güncelle
        user.TodayTaskCount++;
        user.ActiveTaskCount++;
        user.UpdatedAt = DateTime.UtcNow;

        _userTaskWriteRepository.Update(userTask);
        _userWriteRepository.Update(user);
        await _userTaskWriteRepository.SaveAsync();

        await _notificationService.SendNotificationAsync(
            request.UserId,
            "Görev seçildi!",
            $"'{userTask.AppTask?.Title}' görevini seçtin. Hadi başlayalım!",
            "TaskSelected");

        return new UserTaskDto
        {
            Id = userTask.Id,
            Title = userTask.AppTask?.Title ?? string.Empty,
            Description = userTask.AppTask?.Description ?? string.Empty,
            Xp = userTask.EarnedXp,
            BaseXP = userTask.AppTask?.BaseXP ?? 0,
            Difficulty = userTask.AppTask?.Difficulty.ToString() ?? string.Empty,
            IsCompleted = userTask.Status == AppTaskStatus.Completed,
            AssignedAt = userTask.AssignedAt
        };
    }
}
