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
    private readonly IAppUserReadRepository _userReadRepository;
    private readonly IAppUserWriteRepository _userWriteRepository;

    public AssignSpecificTaskCommandHandler(
        IUserTaskWriteRepository userTaskWriteRepository,
        IUserTaskReadRepository userTaskReadRepository,
        IAppTaskReadRepository appTaskReadRepository,
        IAppUserReadRepository userReadRepository,
        IAppUserWriteRepository userWriteRepository)
    {
        _userTaskWriteRepository = userTaskWriteRepository;
        _userTaskReadRepository = userTaskReadRepository;
        _appTaskReadRepository = appTaskReadRepository;
        _userReadRepository = userReadRepository;
        _userWriteRepository = userWriteRepository;
    }

    public async Task<AssignSpecificTaskCommandResponse> Handle(AssignSpecificTaskCommandRequest request, CancellationToken cancellationToken)
    {
        var user = await _userReadRepository.GetByIdAsync(request.UserId.ToString(), tracking: true);
        if (user == null)
        {
            return new AssignSpecificTaskCommandResponse { Success = false, Message = "Kullanıcı bulunamadı." };
        }

        // ─── DailyLimitRule ─────────────────────────────────────────
        if (user.TodayTaskCount >= user.DailyTaskLimit)
        {
            return new AssignSpecificTaskCommandResponse 
            { 
                Success = false, 
                Message = $"Bugün için görev limitine ({user.DailyTaskLimit}) ulaştın. Yarın yeni görev alabilirsin veya limitini ayarlardan değiştirebilirsin." 
            };
        }

        // ─── ActiveLimitRule ────────────────────────────────────────
        if (user.ActiveTaskCount >= user.DailyTaskLimit * 2)
        {
            return new AssignSpecificTaskCommandResponse 
            { 
                Success = false, 
                Message = $"Henüz tamamlanmamış {user.ActiveTaskCount} görevin var. Önce mevcut görevlerini tamamla veya ertele." 
            };
        }

        var taskExists = await _appTaskReadRepository.GetByIdAsync(request.AppTaskId.ToString());
        if (taskExists == null)
        {
            return new AssignSpecificTaskCommandResponse { Success = false, Message = "Görev bulunamadı." };
        }

        var alreadyAssigned = await _userTaskReadRepository.Table
            .AnyAsync(ut => ut.AppUserId == request.UserId && ut.AppTaskId == request.AppTaskId
                && ut.AssignedAt.Date == DateTime.UtcNow.Date
                && ut.Status != AppTaskStatus.Expired && ut.Status != AppTaskStatus.Abandoned, cancellationToken);
            
        if (alreadyAssigned)
        {
            return new AssignSpecificTaskCommandResponse { Success = false, Message = "Bu görev sana bugün zaten atanmış durumda." };
        }

        var newUserTask = new UserTask
        {
            Id = Guid.NewGuid(),
            AppUserId = request.UserId,
            AppTaskId = request.AppTaskId,
            Status = AppTaskStatus.Selected,
            AssignedAt = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddHours(24),
            EarnedXp = 0
        };

        user.TodayTaskCount++;
        user.ActiveTaskCount++;
        user.UpdatedAt = DateTime.UtcNow;

        _userWriteRepository.Update(user);
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
