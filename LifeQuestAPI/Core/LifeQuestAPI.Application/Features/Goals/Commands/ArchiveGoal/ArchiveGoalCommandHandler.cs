using LifeQuestAPI.Application.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LifeQuestAPI.Application.Features.Goals.Commands.ArchiveGoal;

public class ArchiveGoalCommandRequest : IRequest<bool>
{
    public Guid UserId { get; set; }
    public Guid GoalId { get; set; }
}

public sealed class ArchiveGoalCommandHandler : IRequestHandler<ArchiveGoalCommandRequest, bool>
{
    private readonly IUserGoalReadRepository _goalReadRepository;
    private readonly IUserGoalWriteRepository _goalWriteRepository;

    public ArchiveGoalCommandHandler(
        IUserGoalReadRepository goalReadRepository,
        IUserGoalWriteRepository goalWriteRepository)
    {
        _goalReadRepository = goalReadRepository;
        _goalWriteRepository = goalWriteRepository;
    }

    public async Task<bool> Handle(ArchiveGoalCommandRequest request, CancellationToken cancellationToken)
    {
        var goal = await _goalReadRepository
            .GetWhere(g => g.Id == request.GoalId && g.AppUserId == request.UserId, tracking: true)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new Exception("Hedef bulunamadı.");

        goal.IsArchived = true;
        goal.ArchivedAt = DateTime.UtcNow;

        _goalWriteRepository.Update(goal);
        await _goalWriteRepository.SaveAsync();

        return true;
    }
}
