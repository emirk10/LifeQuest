using LifeQuestAPI.Application.Repositories;
using LifeQuestAPI.Domain.Entities;
using LifeQuestAPI.Persistence.Contexts;

namespace LifeQuestAPI.Persistence.Repositories;

public sealed class AchievementReadRepository : ReadRepository<Achievement>, IAchievementReadRepository
{
    public AchievementReadRepository(LifeQuestDbContext context) : base(context)
    {
    }
}
