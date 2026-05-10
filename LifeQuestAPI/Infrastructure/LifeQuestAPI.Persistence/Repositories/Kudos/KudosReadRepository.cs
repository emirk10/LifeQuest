using LifeQuestAPI.Application.Repositories;
using LifeQuestAPI.Domain.Entities;
using LifeQuestAPI.Persistence.Contexts;

namespace LifeQuestAPI.Persistence.Repositories;

public class KudosReadRepository : ReadRepository<Kudos>, IKudosReadRepository
{
    public KudosReadRepository(LifeQuestDbContext context) : base(context)
    {
    }
}
