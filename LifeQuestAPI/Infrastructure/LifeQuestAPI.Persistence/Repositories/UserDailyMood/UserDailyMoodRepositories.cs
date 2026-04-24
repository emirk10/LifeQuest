using LifeQuestAPI.Application.Repositories;
using LifeQuestAPI.Domain.Entities;
using LifeQuestAPI.Persistence.Contexts;

namespace LifeQuestAPI.Persistence.Repositories;

public class UserDailyMoodReadRepository : ReadRepository<UserDailyMood>, IUserDailyMoodReadRepository
{
    public UserDailyMoodReadRepository(LifeQuestDbContext context) : base(context) { }
}

public class UserDailyMoodWriteRepository : WriteRepository<UserDailyMood>, IUserDailyMoodWriteRepository
{
    public UserDailyMoodWriteRepository(LifeQuestDbContext context) : base(context) { }
}
