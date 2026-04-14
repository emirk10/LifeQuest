using LifeQuestAPI.Application.Repositories;
using LifeQuestAPI.Domain.Entities;
using LifeQuestAPI.Persistence.Contexts;

namespace LifeQuestAPI.Persistence.Repositories;

public sealed class TaskVerificationWriteRepository : WriteRepository<TaskVerification>, ITaskVerificationWriteRepository
{
    public TaskVerificationWriteRepository(LifeQuestDbContext context) : base(context)
    {
    }
}
