using LifeQuestAPI.Application.Repositories;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LifeQuestAPI.Application.Features.Goals.Commands.DeleteUserGoal;

public sealed record DeleteUserGoalCommandRequest : IRequest<DeleteUserGoalCommandResponse>
{
    public Guid GoalId { get; set; }
    public Guid UserId { get; set; }
}

public sealed record DeleteUserGoalCommandResponse
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
}

public sealed class DeleteUserGoalCommandHandler : IRequestHandler<DeleteUserGoalCommandRequest, DeleteUserGoalCommandResponse>
{
    private readonly IUserGoalWriteRepository _userGoalWriteRepository;
    private readonly IUserGoalReadRepository _userGoalReadRepository;

    public DeleteUserGoalCommandHandler(IUserGoalWriteRepository userGoalWriteRepository, IUserGoalReadRepository userGoalReadRepository)
    {
        _userGoalWriteRepository = userGoalWriteRepository;
        _userGoalReadRepository = userGoalReadRepository;
    }

    public async Task<DeleteUserGoalCommandResponse> Handle(DeleteUserGoalCommandRequest request, CancellationToken cancellationToken)
    {
        var goal = await _userGoalReadRepository.GetByIdAsync(request.GoalId.ToString(), tracking: true);
        if (goal == null || goal.AppUserId != request.UserId)
        {
            return new DeleteUserGoalCommandResponse { IsSuccess = false, Message = "Hedef bulunamadı." };
        }

        _userGoalWriteRepository.Remove(goal);
        await _userGoalWriteRepository.SaveAsync();

        return new DeleteUserGoalCommandResponse { IsSuccess = true, Message = "Hedef başarıyla silindi." };
    }
}
