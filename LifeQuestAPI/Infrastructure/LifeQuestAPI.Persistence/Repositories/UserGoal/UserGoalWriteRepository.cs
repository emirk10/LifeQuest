using LifeQuestAPI.Application.Repositories;
using LifeQuestAPI.Domain.Entities;
using LifeQuestAPI.Persistence.Contexts;

namespace LifeQuestAPI.Persistence.Repositories;

public sealed class UserGoalWriteRepository : WriteRepository<UserGoal>, IUserGoalWriteRepository
{
    public UserGoalWriteRepository(LifeQuestDbContext context) : base(context)
    {
    }
}
