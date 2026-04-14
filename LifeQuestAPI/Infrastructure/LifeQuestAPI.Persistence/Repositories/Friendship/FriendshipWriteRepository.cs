using LifeQuestAPI.Application.Repositories;
using LifeQuestAPI.Domain.Entities;
using LifeQuestAPI.Persistence.Contexts;

namespace LifeQuestAPI.Persistence.Repositories;

public sealed class FriendshipWriteRepository : WriteRepository<Friendship>, IFriendshipWriteRepository
{
    public FriendshipWriteRepository(LifeQuestDbContext context) : base(context)
    {
    }
}
