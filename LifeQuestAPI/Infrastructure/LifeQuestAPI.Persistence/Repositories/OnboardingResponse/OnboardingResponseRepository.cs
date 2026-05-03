using LifeQuestAPI.Application.Repositories;
using LifeQuestAPI.Domain.Entities;
using LifeQuestAPI.Persistence.Contexts;

namespace LifeQuestAPI.Persistence.Repositories;

public sealed class OnboardingResponseReadRepository : ReadRepository<OnboardingResponse>, IOnboardingResponseReadRepository
{
    public OnboardingResponseReadRepository(LifeQuestDbContext context) : base(context) { }
}

public sealed class OnboardingResponseWriteRepository : WriteRepository<OnboardingResponse>, IOnboardingResponseWriteRepository
{
    public OnboardingResponseWriteRepository(LifeQuestDbContext context) : base(context) { }
}
