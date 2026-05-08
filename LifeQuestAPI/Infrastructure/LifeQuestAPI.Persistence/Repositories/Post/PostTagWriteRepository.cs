using LifeQuestAPI.Application.Repositories;
using LifeQuestAPI.Domain.Entities;
using LifeQuestAPI.Persistence.Contexts;

namespace LifeQuestAPI.Persistence.Repositories;

public sealed class PostTagWriteRepository : WriteRepository<PostTag>, IPostTagWriteRepository
{
    public PostTagWriteRepository(LifeQuestDbContext context) : base(context)
    {
    }
}
