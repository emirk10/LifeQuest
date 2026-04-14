using LifeQuestAPI.Application.Repositories;
using LifeQuestAPI.Domain.Entities;
using LifeQuestAPI.Persistence.Contexts;

namespace LifeQuestAPI.Persistence.Repositories;

public sealed class FriendshipReadRepository : ReadRepository<Friendship>, IFriendshipReadRepository
{
    public FriendshipReadRepository(LifeQuestDbContext context) : base(context)
    {
    }
}
