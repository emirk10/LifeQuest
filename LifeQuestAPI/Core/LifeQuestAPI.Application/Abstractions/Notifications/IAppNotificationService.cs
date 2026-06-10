using System;
using System.Threading.Tasks;

namespace LifeQuestAPI.Application.Abstractions.Notifications;

public interface IAppNotificationService
{
    
    
    
    Task SendNotificationAsync(Guid userId, string title, string message, string? type = null);
}
