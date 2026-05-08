using LifeQuestAPI.Application.Repositories;
using LifeQuestAPI.Domain.Entities;
using LifeQuestAPI.Persistence.Contexts;

namespace LifeQuestAPI.Persistence.Repositories;

public sealed class PostLikeReadRepository : ReadRepository<PostLike>, IPostLikeReadRepository
{
    public PostLikeReadRepository(LifeQuestDbContext context) : base(context)
    {
    }
}
