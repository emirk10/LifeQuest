using LifeQuestAPI.Application.Repositories;
using LifeQuestAPI.Domain.Entities;
using LifeQuestAPI.Persistence.Contexts;

namespace LifeQuestAPI.Persistence.Repositories;

public sealed class TaskVerificationReadRepository : ReadRepository<TaskVerification>, ITaskVerificationReadRepository
{
    public TaskVerificationReadRepository(LifeQuestDbContext context) : base(context)
    {
    }
}
