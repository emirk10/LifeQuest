using LifeQuestAPI.Application.Repositories;
using LifeQuestAPI.Domain.Entities;
using LifeQuestAPI.Persistence.Contexts;

namespace LifeQuestAPI.Persistence.Repositories;

public sealed class UserTaskInteractionReadRepository : ReadRepository<UserTaskInteraction>, IUserTaskInteractionReadRepository
{
    public UserTaskInteractionReadRepository(LifeQuestDbContext context) : base(context)
    {
    }
}
