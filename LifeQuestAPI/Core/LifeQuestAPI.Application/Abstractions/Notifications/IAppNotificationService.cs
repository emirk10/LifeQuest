using System;
using System.Threading.Tasks;

namespace LifeQuestAPI.Application.Abstractions.Notifications;

public interface IAppNotificationService
{
    /// <summary>
    /// Kullanıcıya veritabanı üzerinden bildirim kaydeder ve WebSocket üzerinden anlık iletir.
    /// </summary>
    Task SendNotificationAsync(Guid userId, string title, string message, string? type = null);
}
