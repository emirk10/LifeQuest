using LifeQuestAPI.Application.Repositories;
using LifeQuestAPI.Domain.Entities;
using LifeQuestAPI.Persistence.Contexts;

namespace LifeQuestAPI.Persistence.Repositories;

public sealed class ChallengeParticipantReadRepository : ReadRepository<ChallengeParticipant>, IChallengeParticipantReadRepository
{
    public ChallengeParticipantReadRepository(LifeQuestDbContext context) : base(context) { }
}

public sealed class ChallengeParticipantWriteRepository : WriteRepository<ChallengeParticipant>, IChallengeParticipantWriteRepository
{
    public ChallengeParticipantWriteRepository(LifeQuestDbContext context) : base(context) { }
}
