using LifeQuestAPI.Application.Repositories;
using LifeQuestAPI.Domain.Entities;
using LifeQuestAPI.Persistence.Contexts;

namespace LifeQuestAPI.Persistence.Repositories;

public sealed class UserAchievementWriteRepository : WriteRepository<UserAchievement>, IUserAchievementWriteRepository
{
    public UserAchievementWriteRepository(LifeQuestDbContext context) : base(context)
    {
    }
}
