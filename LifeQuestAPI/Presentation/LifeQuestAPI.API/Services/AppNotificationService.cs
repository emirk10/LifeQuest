using System;
using System.Threading.Tasks;
using LifeQuestAPI.Application.Abstractions.Notifications;
using LifeQuestAPI.Application.Repositories;
using LifeQuestAPI.Domain.Entities;
using Microsoft.AspNetCore.SignalR;

namespace LifeQuestAPI.API.Services;

/// <summary>
/// Bu servis hem veritabanına log düşer hem de SignalR Hub'ını kullanarak anlık push yapar.
/// </summary>
public class AppNotificationService : IAppNotificationService
{
    private readonly INotificationWriteRepository _notificationWriteRepository;
    private readonly IHubContext<Hubs.NotificationHub> _hubContext;

    public AppNotificationService(
        INotificationWriteRepository notificationWriteRepository,
        IHubContext<Hubs.NotificationHub> hubContext)
    {
        _notificationWriteRepository = notificationWriteRepository;
        _hubContext = hubContext;
    }

    public async Task SendNotificationAsync(Guid userId, string title, string message, string? type = null)
    {
        // 1. Veritabanına kaydet
        var notification = new Notification
        {
            AppUserId = userId,
            Title = title,
            Message = message,
            Type = Enum.TryParse<LifeQuestAPI.Domain.Enums.NotificationType>(type, true, out var parsedType) ? parsedType : LifeQuestAPI.Domain.Enums.NotificationType.SystemMessage,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        try
        {
            await _notificationWriteRepository.AddAsync(notification);
            await _notificationWriteRepository.SaveAsync();

            // 2. Anlık olarak (çevrimiçiyse) gönder
            // Grup adı olarak userId kullanıyoruz (Hub içerisinde öyle tanımladık)
            await _hubContext.Clients.Group(userId.ToString()).SendAsync("ReceiveNotification", new 
            {
                Id = notification.Id,
                Title = title,
                Message = message,
                Type = notification.Type.ToString(),
                CreatedAt = notification.CreatedAt
            });
        }
        catch (Exception ex)
        {
            // Loglanabilir
            Console.WriteLine($"Bildirim gönderilirken hata oluştu: {ex.Message}");
        }
    }
}
