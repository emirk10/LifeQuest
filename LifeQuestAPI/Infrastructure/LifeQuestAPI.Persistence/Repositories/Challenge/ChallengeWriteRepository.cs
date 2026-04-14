using LifeQuestAPI.Application.Repositories;
using LifeQuestAPI.Domain.Entities;
using LifeQuestAPI.Persistence.Contexts;

namespace LifeQuestAPI.Persistence.Repositories;

public sealed class ChallengeWriteRepository : WriteRepository<Challenge>, IChallengeWriteRepository
{
    public ChallengeWriteRepository(LifeQuestDbContext context) : base(context)
    {
    }
}
