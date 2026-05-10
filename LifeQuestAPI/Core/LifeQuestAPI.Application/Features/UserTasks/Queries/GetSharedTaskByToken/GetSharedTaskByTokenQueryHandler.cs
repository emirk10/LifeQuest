using LifeQuestAPI.Application.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LifeQuestAPI.Application.Features.UserTasks.Queries.GetSharedTaskByToken;

public sealed class GetSharedTaskByTokenQueryHandler 
    : IRequestHandler<GetSharedTaskByTokenQueryRequest, GetSharedTaskByTokenQueryResponse>
{
    private readonly IUserTaskReadRepository _readRepository;

    public GetSharedTaskByTokenQueryHandler(IUserTaskReadRepository readRepository)
    {
        _readRepository = readRepository;
    }

    public async Task<GetSharedTaskByTokenQueryResponse> Handle(GetSharedTaskByTokenQueryRequest request, CancellationToken cancellationToken)
    {
        var userTask = await _readRepository.Table
            .Include(ut => ut.AppTask)
                .ThenInclude(t => t.Category)
            .Include(ut => ut.AppUser)
            .FirstOrDefaultAsync(ut => ut.ShareToken == request.Token, cancellationToken);

        if (userTask == null)
            return new() { Success = false, Message = "Geçersiz veya süresi dolmuş paylaşım linki." };

        if (userTask.ShareTokenExpiresAt.HasValue && userTask.ShareTokenExpiresAt.Value < DateTime.UtcNow)
            return new() { Success = false, Message = "Bu paylaşım linkinin süresi dolmuş." };

        return new()
        {
            Success = true,
            TaskTitle = userTask.AppTask.Title,
            TaskDescription = userTask.AppTask.Description,
            CategoryName = userTask.AppTask.Category.Name,
            CategoryIcon = userTask.AppTask.Category.IconPath,
            UserName = $"{userTask.AppUser.Name} {userTask.AppUser.Surname}",
            CompletedAt = userTask.CompletedAt,
            ReflectionText = userTask.ReflectionText,
            Tags = userTask.CustomTags ?? userTask.AppTask.Tags
        };
    }
}
