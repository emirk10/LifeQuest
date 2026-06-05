using LifeQuestAPI.Application.Repositories;
using LifeQuestAPI.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LifeQuestAPI.Application.Features.UserTasks.Commands.CancelUserTask;

public class CancelUserTaskCommandRequest : IRequest<bool>
{
    public Guid UserId { get; set; }
    public Guid UserTaskId { get; set; }
}

public class CancelUserTaskCommandHandler : IRequestHandler<CancelUserTaskCommandRequest, bool>
{
    private readonly IUserTaskReadRepository _readRepository;
    private readonly IUserTaskWriteRepository _writeRepository;
    private readonly IAppUserReadRepository _userReadRepository;
    private readonly IAppUserWriteRepository _userWriteRepository;

    public CancelUserTaskCommandHandler(
        IUserTaskReadRepository readRepository, 
        IUserTaskWriteRepository writeRepository,
        IAppUserReadRepository userReadRepository,
        IAppUserWriteRepository userWriteRepository)
    {
        _readRepository = readRepository;
        _writeRepository = writeRepository;
        _userReadRepository = userReadRepository;
        _userWriteRepository = userWriteRepository;
    }

    public async Task<bool> Handle(CancelUserTaskCommandRequest request, CancellationToken cancellationToken)
    {
        var userTask = await _readRepository
            .GetWhere(ut => ut.Id == request.UserTaskId && ut.AppUserId == request.UserId, tracking: true)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new Exception("Görev bulunamadı.");

        // Yalnızca tamamlanmamış, ertelenmemiş ve vazgeçilmemiş görevler iptal edilebilir.
        if (userTask.Status == AppTaskStatus.Completed || userTask.Status == AppTaskStatus.Abandoned || userTask.Status == AppTaskStatus.Cancelled)
            throw new InvalidOperationException("Bu durumdaki görevler iptal edilemez.");

        userTask.Status = AppTaskStatus.Cancelled;

        var user = await _userReadRepository.GetByIdAsync(request.UserId.ToString(), tracking: true);
        if (user != null && user.ActiveTaskCount > 0)
        {
            user.ActiveTaskCount--;
            _userWriteRepository.Update(user);
        }

        _writeRepository.Update(userTask);
        await _writeRepository.SaveAsync();

        return true;
    }
}
