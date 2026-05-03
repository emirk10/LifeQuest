using LifeQuestAPI.Application.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LifeQuestAPI.Application.Features.Goals.Queries.GetGoalDashboard;

public class GetGoalDashboardQueryRequest : IRequest<GetGoalDashboardQueryResponse>
{
    public Guid UserId { get; set; }
}

public class GoalDashboardItem
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public string? Color { get; set; }
    public int CurrentValue { get; set; }
    public int TargetValue { get; set; }
    public int ProgressPercent => TargetValue > 0 ? (int)((double)CurrentValue / TargetValue * 100) : 0;
    public string ProgressUnit { get; set; } = "görev";
    public bool IsCompleted { get; set; }
    public DateTime EndDate { get; set; }
    public int DaysRemaining => Math.Max(0, (int)(EndDate - DateTime.UtcNow).TotalDays);
    public bool IsOnLag { get; set; }
    public int MilestoneCount { get; set; }
    public int CompletedMilestoneCount { get; set; }
    public string? Why { get; set; }
    public string Type { get; set; } = string.Empty;
}

public class GetGoalDashboardQueryResponse
{
    public List<GoalDashboardItem> ActiveGoals { get; set; } = new();
    public List<GoalDashboardItem> CompletedGoals { get; set; } = new();
    public List<GoalDashboardItem> ArchivedGoals { get; set; } = new();
    public int TotalActiveGoals { get; set; }
    public int TotalCompletedThisMonth { get; set; }
}

public sealed class GetGoalDashboardQueryHandler
    : IRequestHandler<GetGoalDashboardQueryRequest, GetGoalDashboardQueryResponse>
{
    private readonly IUserGoalReadRepository _goalReadRepository;
    private readonly IGoalMilestoneReadRepository _milestoneReadRepository;

    public GetGoalDashboardQueryHandler(
        IUserGoalReadRepository goalReadRepository,
        IGoalMilestoneReadRepository milestoneReadRepository)
    {
        _goalReadRepository = goalReadRepository;
        _milestoneReadRepository = milestoneReadRepository;
    }

    public async Task<GetGoalDashboardQueryResponse> Handle(
        GetGoalDashboardQueryRequest request, CancellationToken cancellationToken)
    {
        var goals = await _goalReadRepository
            .GetWhere(g => g.AppUserId == request.UserId, tracking: false)
            .Include(g => g.Milestones)
            .OrderByDescending(g => g.StartDate)
            .ToListAsync(cancellationToken);

        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var mapGoal = (Domain.Entities.UserGoal g) =>
        {
            var elapsed = (now - g.StartDate).TotalDays;
            var total = (g.EndDate - g.StartDate).TotalDays;
            var expected = total > 0 ? elapsed / total : 0;
            var actual = g.TargetValue > 0 ? (double)g.CurrentValue / g.TargetValue : 0;
            var isOnLag = actual < expected * 0.8;

            return new GoalDashboardItem
            {
                Id = g.Id,
                Title = g.Title,
                Icon = g.Icon,
                Color = g.Color,
                CurrentValue = g.CurrentValue,
                TargetValue = g.TargetValue,
                ProgressUnit = g.ProgressUnit,
                IsCompleted = g.IsCompleted,
                EndDate = g.EndDate,
                IsOnLag = !g.IsCompleted && isOnLag,
                MilestoneCount = g.Milestones?.Count ?? 0,
                CompletedMilestoneCount = g.Milestones?.Count(m => m.IsCompleted) ?? 0,
                Why = g.Why,
                Type = g.Type.ToString()
            };
        };

        var activeGoals = goals.Where(g => !g.IsCompleted && !g.IsArchived).Select(mapGoal).ToList();
        var completedGoals = goals.Where(g => g.IsCompleted).Select(mapGoal).ToList();
        var archivedGoals = goals.Where(g => g.IsArchived && !g.IsCompleted).Select(mapGoal).ToList();

        return new GetGoalDashboardQueryResponse
        {
            ActiveGoals = activeGoals,
            CompletedGoals = completedGoals,
            ArchivedGoals = archivedGoals,
            TotalActiveGoals = activeGoals.Count,
            TotalCompletedThisMonth = goals.Count(g => g.IsCompleted && g.CompletedAt >= startOfMonth)
        };
    }
}
