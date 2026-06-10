using LifeQuestAPI.Application.Abstractions;
using LifeQuestAPI.Application.DTOs;
using LifeQuestAPI.Application.Repositories;
using LifeQuestAPI.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LifeQuestAPI.Application.Features.UserTasks.Commands.PauseResumeTask;


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
    private readonly IAppUserReadRepository _userReadRepository;
    private readonly IAppUserWriteRepository _userWriteRepository;
    private readonly IUserBehaviorAnalysisService _behaviorAnalysisService;

    public AbandonTaskCommandHandler(
        IUserTaskReadRepository readRepository, 
        IUserTaskWriteRepository writeRepository,
        IAppUserReadRepository userReadRepository,
        IAppUserWriteRepository userWriteRepository,
        IUserBehaviorAnalysisService behaviorAnalysisService)
    {
        _readRepository = readRepository;
        _writeRepository = writeRepository;
        _userReadRepository = userReadRepository;
        _userWriteRepository = userWriteRepository;
        _behaviorAnalysisService = behaviorAnalysisService;
    }

    public async Task<bool> Handle(AbandonTaskCommandRequest request, CancellationToken cancellationToken)
    {
        var userTask = await _readRepository
            .GetWhere(ut => ut.Id == request.UserTaskId && ut.AppUserId == request.UserId, tracking: true)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new Exception("Görev bulunamadı.");

        userTask.Status = AppTaskStatus.Abandoned;
        userTask.SkipReason = request.Reason;

        var user = await _userReadRepository.GetByIdAsync(request.UserId.ToString(), tracking: true);
        if (user != null && user.ActiveTaskCount > 0)
        {
            user.ActiveTaskCount--;
            _userWriteRepository.Update(user);
        }

        _writeRepository.Update(userTask);
        await _writeRepository.SaveAsync();

        
        try
        {
            await _behaviorAnalysisService.AnalyzeAndUpdateUserBehaviorAsync(request.UserId, cancellationToken);
        }
        catch
        {
            
        }

        return true;
    }
}


public class StartTaskCommandRequest : IRequest<UserTaskDto>
{
    public Guid UserId { get; set; }
    public Guid UserTaskId { get; set; }
}

public class StartTaskCommandHandler : IRequestHandler<StartTaskCommandRequest, UserTaskDto>
{
    private readonly IUserTaskReadRepository _readRepository;
    private readonly IUserTaskWriteRepository _writeRepository;

    public StartTaskCommandHandler(IUserTaskReadRepository readRepository, IUserTaskWriteRepository writeRepository)
    {
        _readRepository = readRepository;
        _writeRepository = writeRepository;
    }

    public async Task<UserTaskDto> Handle(StartTaskCommandRequest request, CancellationToken cancellationToken)
    {
        var userTask = await _readRepository
            .GetWhere(ut => ut.Id == request.UserTaskId && ut.AppUserId == request.UserId, tracking: true)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new Exception("Görev bulunamadı.");

        if (userTask.Status != AppTaskStatus.Selected && userTask.Status != AppTaskStatus.Suggested)
            throw new InvalidOperationException("Yalnızca seçilmiş veya önerilmiş görevler başlatılabilir.");

        userTask.Status = AppTaskStatus.InProgress;

        _writeRepository.Update(userTask);
        await _writeRepository.SaveAsync();

        return new UserTaskDto { Id = userTask.Id, IsCompleted = false, Xp = userTask.EarnedXp, AssignedAt = userTask.AssignedAt };
    }
}
