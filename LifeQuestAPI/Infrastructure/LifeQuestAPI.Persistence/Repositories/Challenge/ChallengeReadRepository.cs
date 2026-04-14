using LifeQuestAPI.Application.Repositories;
using LifeQuestAPI.Domain.Entities;
using LifeQuestAPI.Persistence.Contexts;

namespace LifeQuestAPI.Persistence.Repositories;

public sealed class ChallengeReadRepository : ReadRepository<Challenge>, IChallengeReadRepository
{
    public ChallengeReadRepository(LifeQuestDbContext context) : base(context)
    {
    }
}
