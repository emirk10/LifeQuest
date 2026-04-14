using LifeQuestAPI.Application.Repositories;
using LifeQuestAPI.Domain.Entities;
using LifeQuestAPI.Persistence.Contexts;

namespace LifeQuestAPI.Persistence.Repositories;

public sealed class UserAchievementReadRepository : ReadRepository<UserAchievement>, IUserAchievementReadRepository
{
    public UserAchievementReadRepository(LifeQuestDbContext context) : base(context)
    {
    }
}
