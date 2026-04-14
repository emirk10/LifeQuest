using LifeQuestAPI.Application.Repositories;
using LifeQuestAPI.Domain.Entities;
using LifeQuestAPI.Persistence.Contexts;

namespace LifeQuestAPI.Persistence.Repositories;

public sealed class UserTaskInteractionWriteRepository : WriteRepository<UserTaskInteraction>, IUserTaskInteractionWriteRepository
{
    public UserTaskInteractionWriteRepository(LifeQuestDbContext context) : base(context)
    {
    }
}
