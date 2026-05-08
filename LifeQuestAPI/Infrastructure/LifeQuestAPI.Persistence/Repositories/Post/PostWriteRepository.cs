using LifeQuestAPI.Application.Repositories;
using LifeQuestAPI.Domain.Entities;
using LifeQuestAPI.Persistence.Contexts;

namespace LifeQuestAPI.Persistence.Repositories;

public sealed class PostWriteRepository : WriteRepository<Post>, IPostWriteRepository
{
    public PostWriteRepository(LifeQuestDbContext context) : base(context)
    {
    }
}
