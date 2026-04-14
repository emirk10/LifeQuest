using LifeQuestAPI.Application.Repositories;
using LifeQuestAPI.Domain.Entities;
using LifeQuestAPI.Persistence.Contexts;

namespace LifeQuestAPI.Persistence.Repositories;

public sealed class NotificationWriteRepository : WriteRepository<Notification>, INotificationWriteRepository
{
    public NotificationWriteRepository(LifeQuestDbContext context) : base(context)
    {
    }
}
