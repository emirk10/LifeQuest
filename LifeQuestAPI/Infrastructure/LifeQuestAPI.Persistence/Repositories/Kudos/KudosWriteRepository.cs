using LifeQuestAPI.Application.Repositories;
using LifeQuestAPI.Domain.Entities;
using LifeQuestAPI.Persistence.Contexts;

namespace LifeQuestAPI.Persistence.Repositories;

public class KudosWriteRepository : WriteRepository<Kudos>, IKudosWriteRepository
{
    public KudosWriteRepository(LifeQuestDbContext context) : base(context)
    {
    }
}
