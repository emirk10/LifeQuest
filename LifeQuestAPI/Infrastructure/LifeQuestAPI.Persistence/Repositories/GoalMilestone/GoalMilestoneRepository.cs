using LifeQuestAPI.Application.Repositories;
using LifeQuestAPI.Domain.Entities;
using LifeQuestAPI.Persistence.Contexts;

namespace LifeQuestAPI.Persistence.Repositories;

public sealed class GoalMilestoneReadRepository : ReadRepository<GoalMilestone>, IGoalMilestoneReadRepository
{
    public GoalMilestoneReadRepository(LifeQuestDbContext context) : base(context) { }
}

public sealed class GoalMilestoneWriteRepository : WriteRepository<GoalMilestone>, IGoalMilestoneWriteRepository
{
    public GoalMilestoneWriteRepository(LifeQuestDbContext context) : base(context) { }
}
