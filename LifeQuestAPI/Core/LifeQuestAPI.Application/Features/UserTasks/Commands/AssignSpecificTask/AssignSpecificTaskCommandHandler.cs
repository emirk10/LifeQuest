using LifeQuestAPI.Application.Repositories;
using LifeQuestAPI.Domain.Entities;
using LifeQuestAPI.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LifeQuestAPI.Application.Features.UserTasks.Commands.AssignSpecificTask;

public class AssignSpecificTaskCommandHandler : IRequestHandler<AssignSpecificTaskCommandRequest, AssignSpecificTaskCommandResponse>
{
    private readonly IUserTaskWriteRepository _userTaskWriteRepository;
    private readonly IUserTaskReadRepository _userTaskReadRepository;
    private readonly IAppTaskReadRepository _appTaskReadRepository;

    public AssignSpecificTaskCommandHandler(
        IUserTaskWriteRepository userTaskWriteRepository,
        IUserTaskReadRepository userTaskReadRepository,
        IAppTaskReadRepository appTaskReadRepository)
    {
        _userTaskWriteRepository = userTaskWriteRepository;
        _userTaskReadRepository = userTaskReadRepository;
        _appTaskReadRepository = appTaskReadRepository;
    }

    public async Task<AssignSpecificTaskCommandResponse> Handle(AssignSpecificTaskCommandRequest request, CancellationToken cancellationToken)
    {
        var taskExists = await _appTaskReadRepository.GetByIdAsync(request.AppTaskId.ToString());
        if (taskExists == null)
        {
            return new AssignSpecificTaskCommandResponse { Success = false, Message = "Görev bulunamadı." };
        }

        var alreadyAssigned = await _userTaskReadRepository.Table
            .AnyAsync(ut => ut.AppUserId == request.UserId && ut.AppTaskId == request.AppTaskId && ut.Status != AppTaskStatus.Failed, cancellationToken);
            
        if (alreadyAssigned)
        {
            return new AssignSpecificTaskCommandResponse { Success = false, Message = "Bu görev sana zaten atanmış durumda." };
        }

        var newUserTask = new UserTask
        {
            Id = Guid.NewGuid(),
            AppUserId = request.UserId,
            AppTaskId = request.AppTaskId,
            Status = AppTaskStatus.Assigned,
            AssignedAt = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddHours(24),
            EarnedXp = 0
        };

        await _userTaskWriteRepository.AddAsync(newUserTask);
        await _userTaskWriteRepository.SaveAsync();

        return new AssignSpecificTaskCommandResponse
        {
            Success = true,
            Message = "Görev başarıyla eklendi!",
            AssignedUserTaskId = newUserTask.Id
        };
    }
}
