using LifeQuestAPI.Application.Repositories;
using LifeQuestAPI.Domain.Entities;
using LifeQuestAPI.Persistence.Contexts;

namespace LifeQuestAPI.Persistence.Repositories;

public class PostTagReadRepository : ReadRepository<PostTag>, IPostTagReadRepository
{
    public PostTagReadRepository(LifeQuestDbContext context) : base(context)
    {
    }
}
