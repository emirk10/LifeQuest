using LifeQuestAPI.Application.Repositories;
using LifeQuestAPI.Domain.Entities;
using LifeQuestAPI.Persistence.Contexts;

namespace LifeQuestAPI.Persistence.Repositories;

public sealed class PostLikeWriteRepository : WriteRepository<PostLike>, IPostLikeWriteRepository
{
    public PostLikeWriteRepository(LifeQuestDbContext context) : base(context)
    {
    }
}
