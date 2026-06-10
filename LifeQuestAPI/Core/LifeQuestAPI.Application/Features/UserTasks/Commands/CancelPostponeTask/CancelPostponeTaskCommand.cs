using LifeQuestAPI.Application.DTOs;
using LifeQuestAPI.Application.Repositories;
using LifeQuestAPI.Domain.Entities;
using LifeQuestAPI.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LifeQuestAPI.Application.Features.UserTasks.Commands.CancelPostponeTask;

public class CancelPostponeTaskCommandRequest : IRequest<bool>
{
    public Guid UserId { get; set; }
    public Guid UserTaskId { get; set; }
}

public class CancelPostponeTaskCommandHandler : IRequestHandler<CancelPostponeTaskCommandRequest, bool>
{
    private readonly IUserTaskReadRepository _userTaskReadRepository;
    private readonly IUserTaskWriteRepository _userTaskWriteRepository;
    private readonly IAppUserReadRepository _userReadRepository;
    private readonly IAppUserWriteRepository _userWriteRepository;

    public CancelPostponeTaskCommandHandler(
        IUserTaskReadRepository userTaskReadRepository,
        IUserTaskWriteRepository userTaskWriteRepository,
        IAppUserReadRepository userReadRepository,
        IAppUserWriteRepository userWriteRepository)
    {
        _userTaskReadRepository = userTaskReadRepository;
        _userTaskWriteRepository = userTaskWriteRepository;
        _userReadRepository = userReadRepository;
        _userWriteRepository = userWriteRepository;
    }

    public async Task<bool> Handle(CancelPostponeTaskCommandRequest request, CancellationToken cancellationToken)
    {
        var userTask = await _userTaskReadRepository
            .GetWhere(ut => ut.Id == request.UserTaskId && ut.AppUserId == request.UserId, tracking: true)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new Exception("Görev bulunamadı.");

        if (!userTask.IsPostponed || userTask.Status != AppTaskStatus.Postponed)
            throw new InvalidOperationException("Bu görev ertelenmiş durumda değil.");

        
        if (userTask.PostponedTo.HasValue)
        {
            var dueDate = userTask.PostponedTo.Value.ToDateTime(TimeOnly.MinValue);
            
            dueDate = DateTime.SpecifyKind(dueDate, DateTimeKind.Utc);
            
            var tomorrowTask = await _userTaskReadRepository
                .GetWhere(ut => ut.AppUserId == request.UserId 
                             && ut.AppTaskId == userTask.AppTaskId 
                             && ut.Status == AppTaskStatus.Suggested
                             && ut.DueDate == dueDate, tracking: true)
                .FirstOrDefaultAsync(cancellationToken);

            if (tomorrowTask != null)
            {
                _userTaskWriteRepository.Remove(tomorrowTask);
            }
        }

        
        userTask.IsPostponed = false;
        userTask.PostponeCount = Math.Max(0, userTask.PostponeCount - 1);
        userTask.PostponedTo = null;
        userTask.Status = AppTaskStatus.Selected; 
        userTask.SkipReason = null;
        userTask.XpCompensationRate = 1.0m;

        _userTaskWriteRepository.Update(userTask);

        
        var user = await _userReadRepository.GetByIdAsync(request.UserId.ToString(), tracking: true);
        if (user != null)
        {
            user.TotalPostponeCount = Math.Max(0, user.TotalPostponeCount - 1);
            _userWriteRepository.Update(user);
        }

        await _userTaskWriteRepository.SaveAsync();

        return true;
    }
}
