using LifeQuestAPI.Application.Repositories;
using LifeQuestAPI.Domain.Entities;
using LifeQuestAPI.Persistence.Contexts;

namespace LifeQuestAPI.Persistence.Repositories;

public sealed class UserGoalReadRepository : ReadRepository<UserGoal>, IUserGoalReadRepository
{
    public UserGoalReadRepository(LifeQuestDbContext context) : base(context)
    {
    }
}
