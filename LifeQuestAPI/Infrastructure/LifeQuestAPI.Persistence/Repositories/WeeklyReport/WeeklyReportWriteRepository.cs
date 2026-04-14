using LifeQuestAPI.Application.Repositories;
using LifeQuestAPI.Domain.Entities;
using LifeQuestAPI.Persistence.Contexts;

namespace LifeQuestAPI.Persistence.Repositories;

public sealed class WeeklyReportWriteRepository : WriteRepository<WeeklyReport>, IWeeklyReportWriteRepository
{
    public WeeklyReportWriteRepository(LifeQuestDbContext context) : base(context)
    {
    }
}
