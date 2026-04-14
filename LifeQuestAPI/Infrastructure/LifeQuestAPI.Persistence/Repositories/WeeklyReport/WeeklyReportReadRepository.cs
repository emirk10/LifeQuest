using LifeQuestAPI.Application.Repositories;
using LifeQuestAPI.Domain.Entities;
using LifeQuestAPI.Persistence.Contexts;

namespace LifeQuestAPI.Persistence.Repositories;

public sealed class WeeklyReportReadRepository : ReadRepository<WeeklyReport>, IWeeklyReportReadRepository
{
    public WeeklyReportReadRepository(LifeQuestDbContext context) : base(context)
    {
    }
}
