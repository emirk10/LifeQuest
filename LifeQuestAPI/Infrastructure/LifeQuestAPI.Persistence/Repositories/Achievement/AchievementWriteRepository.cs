using LifeQuestAPI.Application.Repositories;
using LifeQuestAPI.Domain.Entities;
using LifeQuestAPI.Persistence.Contexts;

namespace LifeQuestAPI.Persistence.Repositories;

public sealed class AchievementWriteRepository : WriteRepository<Achievement>, IAchievementWriteRepository
{
    public AchievementWriteRepository(LifeQuestDbContext context) : base(context)
    {
    }
}
