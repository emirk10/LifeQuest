using LifeQuestAPI.Application.Repositories;
using LifeQuestAPI.Domain.Entities;
using LifeQuestAPI.Persistence.Contexts;

namespace LifeQuestAPI.Persistence.Repositories;

public sealed class PostReadRepository : ReadRepository<Post>, IPostReadRepository
{
    public PostReadRepository(LifeQuestDbContext context) : base(context)
    {
    }
}
