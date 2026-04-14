using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LifeQuestAPI.Application.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LifeQuestAPI.Application.Features.Goals.Queries.GetUserGoals;

public class GetUserGoalsQueryHandler : IRequestHandler<GetUserGoalsQueryRequest, GetUserGoalsQueryResponse>
{
    private readonly IUserGoalReadRepository _goalReadRepository;

    public GetUserGoalsQueryHandler(IUserGoalReadRepository goalReadRepository)
    {
        _goalReadRepository = goalReadRepository;
    }

    public async Task<GetUserGoalsQueryResponse> Handle(GetUserGoalsQueryRequest request, CancellationToken cancellationToken)
    {
        var goals = await _goalReadRepository.Table
            .Include(g => g.Category)
            .Where(g => g.AppUserId == request.UserId)
            .OrderByDescending(g => g.CreatedAt)
            .Select(g => new GoalDto
            {
                Id = g.Id,
                Title = g.Title,
                Description = g.Description,
                Type = g.Type,
                TargetValue = g.TargetValue,
                CurrentValue = g.CurrentValue,
                CategoryName = g.Category != null ? g.Category.Name : null,
                StartDate = g.StartDate,
                EndDate = g.EndDate,
                IsCompleted = g.IsCompleted,
                BonusXP = g.BonusXP
            })
            .ToListAsync(cancellationToken);

        return new GetUserGoalsQueryResponse
        {
            Success = true,
            Goals = goals
        };
    }
}
