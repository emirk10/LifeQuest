using LifeQuestAPI.Application.Repositories;
using LifeQuestAPI.Domain.Entities;
using LifeQuestAPI.Persistence.Contexts;

namespace LifeQuestAPI.Persistence.Repositories;

public sealed class PostCommentReadRepository : ReadRepository<PostComment>, IPostCommentReadRepository
{
    public PostCommentReadRepository(LifeQuestDbContext context) : base(context)
    {
    }
}
