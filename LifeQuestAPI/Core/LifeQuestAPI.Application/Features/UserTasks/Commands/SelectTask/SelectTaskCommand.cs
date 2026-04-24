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
}

public class SelectTaskCommandHandler : IRequestHandler<SelectTaskCommandRequest, UserTaskDto>
{
    private readonly IUserTaskReadRepository _userTaskReadRepository;
    private readonly IUserTaskWriteRepository _userTaskWriteRepository;
    private readonly IUserDailyMoodReadRepository _moodReadRepository;

    public SelectTaskCommandHandler(
        IUserTaskReadRepository userTaskReadRepository,
        IUserTaskWriteRepository userTaskWriteRepository,
        IUserDailyMoodReadRepository moodReadRepository)
    {
        _userTaskReadRepository = userTaskReadRepository;
        _userTaskWriteRepository = userTaskWriteRepository;
        _moodReadRepository = moodReadRepository;
    }

    public async Task<UserTaskDto> Handle(SelectTaskCommandRequest request, CancellationToken cancellationToken)
    {
        var userTask = await _userTaskReadRepository
            .GetWhere(ut => ut.Id == request.UserTaskId && ut.AppUserId == request.UserId, tracking: true)
            .Include(ut => ut.AppTask)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new Exception("Görev bulunamadı.");

        if (userTask.Status != AppTaskStatus.Suggested)
            throw new Exception("Yalnızca önerilmiş görevler seçilebilir.");

        // Bugünkü mood'dan energy ve mood snapshot al
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var todayMood = await _moodReadRepository
            .GetWhere(m => m.AppUserId == request.UserId && m.Date == today, tracking: false)
            .FirstOrDefaultAsync(cancellationToken);

        userTask.Status = AppTaskStatus.Selected;
        userTask.UserEnergyAtStart = todayMood?.EnergyLevel;
        userTask.UserMoodAtStart = todayMood?.MoodMode;

        _userTaskWriteRepository.Update(userTask);
        await _userTaskWriteRepository.SaveAsync();

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
