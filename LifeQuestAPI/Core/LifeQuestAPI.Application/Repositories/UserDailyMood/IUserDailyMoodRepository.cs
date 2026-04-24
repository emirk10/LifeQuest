using LifeQuestAPI.Application.Repositories;
using LifeQuestAPI.Domain.Entities;

namespace LifeQuestAPI.Application.Repositories;

public interface IUserDailyMoodReadRepository : IReadRepository<UserDailyMood> { }
public interface IUserDailyMoodWriteRepository : IWriteRepository<UserDailyMood> { }
