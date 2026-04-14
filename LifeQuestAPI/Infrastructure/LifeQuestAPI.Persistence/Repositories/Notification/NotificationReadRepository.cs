using LifeQuestAPI.Application.Repositories;
using LifeQuestAPI.Domain.Entities;
using LifeQuestAPI.Persistence.Contexts;

namespace LifeQuestAPI.Persistence.Repositories;

public sealed class NotificationReadRepository : ReadRepository<Notification>, INotificationReadRepository
{
    public NotificationReadRepository(LifeQuestDbContext context) : base(context)
    {
    }
}
