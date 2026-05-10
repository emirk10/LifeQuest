using LifeQuestAPI.Application.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LifeQuestAPI.Application.Features.UserTasks.Commands.ShareUserTask;

public sealed class ShareUserTaskCommandHandler 
    : IRequestHandler<ShareUserTaskCommandRequest, ShareUserTaskCommandResponse>
{
    private readonly IUserTaskReadRepository _readRepository;
    private readonly IUserTaskWriteRepository _writeRepository;

    public ShareUserTaskCommandHandler(IUserTaskReadRepository readRepository, IUserTaskWriteRepository writeRepository)
    {
        _readRepository = readRepository;
        _writeRepository = writeRepository;
    }

    public async Task<ShareUserTaskCommandResponse> Handle(ShareUserTaskCommandRequest request, CancellationToken cancellationToken)
    {
        var userTask = await _readRepository.Table
            .FirstOrDefaultAsync(ut => ut.Id == request.UserTaskId && ut.AppUserId == request.UserId, cancellationToken);

        if (userTask == null)
            return new() { Success = false, Message = "Görev bulunamadı." };

        // Token oluştur (UUID)
        userTask.ShareToken = Guid.NewGuid().ToString("N");
        userTask.ShareTokenExpiresAt = DateTime.UtcNow.AddDays(7);

        _writeRepository.Update(userTask);
        await _writeRepository.SaveAsync();

        return new()
        {
            Success = true,
            Message = "Paylaşım linki oluşturuldu.",
            ShareToken = userTask.ShareToken,
            ShareUrl = $"/share/{userTask.ShareToken}" // Frontend route
        };
    }
}
