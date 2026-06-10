using System;
using System.Threading.Tasks;
using LifeQuestAPI.Application.Abstractions.Notifications;
using LifeQuestAPI.Application.Repositories;
using LifeQuestAPI.Domain.Entities;
using Microsoft.AspNetCore.SignalR;

namespace LifeQuestAPI.API.Services;




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
            
            Console.WriteLine($"Bildirim gönderilirken hata oluştu: {ex.Message}");
        }
    }
}
