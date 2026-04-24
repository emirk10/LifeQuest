using LifeQuestAPI.Application.DTOs;
using LifeQuestAPI.Application.Repositories;
using LifeQuestAPI.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LifeQuestAPI.Application.Features.UserTasks.Commands.PauseResumeTask;

// ─── Pause ────────────────────────────────────────────────────────────────────
public class PauseTaskCommandRequest : IRequest<UserTaskDto>
{
    public Guid UserId { get; set; }
    public Guid UserTaskId { get; set; }
}

public class PauseTaskCommandHandler : IRequestHandler<PauseTaskCommandRequest, UserTaskDto>
{
    private readonly IUserTaskReadRepository _readRepository;
    private readonly IUserTaskWriteRepository _writeRepository;

    public PauseTaskCommandHandler(IUserTaskReadRepository readRepository, IUserTaskWriteRepository writeRepository)
    {
        _readRepository = readRepository;
        _writeRepository = writeRepository;
    }

    public async Task<UserTaskDto> Handle(PauseTaskCommandRequest request, CancellationToken cancellationToken)
    {
        var userTask = await _readRepository
            .GetWhere(ut => ut.Id == request.UserTaskId && ut.AppUserId == request.UserId, tracking: true)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new Exception("Görev bulunamadı.");

        if (userTask.Status != AppTaskStatus.InProgress)
            throw new InvalidOperationException("Yalnızca devam eden görevler duraklatılabilir.");

        userTask.Status = AppTaskStatus.Paused;
        userTask.PausedAt = DateTime.UtcNow;

        _writeRepository.Update(userTask);
        await _writeRepository.SaveAsync();

        return new UserTaskDto { Id = userTask.Id, IsCompleted = false, Xp = userTask.EarnedXp, AssignedAt = userTask.AssignedAt };
    }
}

// ─── Resume ───────────────────────────────────────────────────────────────────
public class ResumeTaskCommandRequest : IRequest<UserTaskDto>
{
    public Guid UserId { get; set; }
    public Guid UserTaskId { get; set; }
}

public class ResumeTaskCommandHandler : IRequestHandler<ResumeTaskCommandRequest, UserTaskDto>
{
    private readonly IUserTaskReadRepository _readRepository;
    private readonly IUserTaskWriteRepository _writeRepository;

    public ResumeTaskCommandHandler(IUserTaskReadRepository readRepository, IUserTaskWriteRepository writeRepository)
    {
        _readRepository = readRepository;
        _writeRepository = writeRepository;
    }

    public async Task<UserTaskDto> Handle(ResumeTaskCommandRequest request, CancellationToken cancellationToken)
    {
        var userTask = await _readRepository
            .GetWhere(ut => ut.Id == request.UserTaskId && ut.AppUserId == request.UserId, tracking: true)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new Exception("Görev bulunamadı.");

        if (userTask.Status != AppTaskStatus.Paused)
            throw new InvalidOperationException("Yalnızca duraklatılmış görevler devam ettirilebilir.");

        if (userTask.PausedAt.HasValue)
        {
            var pausedMinutes = (int)(DateTime.UtcNow - userTask.PausedAt.Value).TotalMinutes;
            userTask.TotalPausedMinutes += pausedMinutes;
        }

        userTask.Status = AppTaskStatus.InProgress;
        userTask.PausedAt = null;

        _writeRepository.Update(userTask);
        await _writeRepository.SaveAsync();

        return new UserTaskDto { Id = userTask.Id, IsCompleted = false, Xp = userTask.EarnedXp, AssignedAt = userTask.AssignedAt };
    }
}

// ─── Abandon ──────────────────────────────────────────────────────────────────
public class AbandonTaskCommandRequest : IRequest<bool>
{
    public Guid UserId { get; set; }
    public Guid UserTaskId { get; set; }
    public string? Reason { get; set; }
}

public class AbandonTaskCommandHandler : IRequestHandler<AbandonTaskCommandRequest, bool>
{
    private readonly IUserTaskReadRepository _readRepository;
    private readonly IUserTaskWriteRepository _writeRepository;

    public AbandonTaskCommandHandler(IUserTaskReadRepository readRepository, IUserTaskWriteRepository writeRepository)
    {
        _readRepository = readRepository;
        _writeRepository = writeRepository;
    }

    public async Task<bool> Handle(AbandonTaskCommandRequest request, CancellationToken cancellationToken)
    {
        var userTask = await _readRepository
            .GetWhere(ut => ut.Id == request.UserTaskId && ut.AppUserId == request.UserId, tracking: true)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new Exception("Görev bulunamadı.");

        userTask.Status = AppTaskStatus.Abandoned;
        userTask.SkipReason = request.Reason;

        _writeRepository.Update(userTask);
        await _writeRepository.SaveAsync();

        return true;
    }
}
