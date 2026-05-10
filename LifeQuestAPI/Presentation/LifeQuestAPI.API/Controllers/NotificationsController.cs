using System;
using System.Security.Claims;
using System.Threading.Tasks;
using LifeQuestAPI.Application.Abstractions.Notifications;
using LifeQuestAPI.Application.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace LifeQuestAPI.API.Controllers;

[Route("api/notifications")]
[ApiController]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationReadRepository _notificationRead;
    private readonly INotificationWriteRepository _notificationWrite;

    public NotificationsController(
        INotificationReadRepository notificationRead,
        INotificationWriteRepository notificationWrite)
    {
        _notificationRead = notificationRead;
        _notificationWrite = notificationWrite;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyNotifications()
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString)) return Unauthorized();

        var userId = Guid.Parse(userIdString);

        var notifications = await _notificationRead.GetAll(false)
            .Where(n => n.AppUserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(100)
            .ToListAsync();

        var unreadCount = await _notificationRead.GetAll(false)
            .CountAsync(n => n.AppUserId == userId && !n.IsRead);

        var grouped = notifications
            .GroupBy(n => n.Type)
            .ToDictionary(g => g.Key.ToString(), g => g.ToList());

        return Ok(new 
        { 
            success = true, 
            unreadCount,
            notifications, // Düz liste de kalsın
            grouped 
        });
    }

    [HttpPatch("{id}/mark-as-read")]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString)) return Unauthorized();

        var userId = Guid.Parse(userIdString);

        var notification = await _notificationRead.GetByIdAsync(id.ToString());
        if (notification == null) return NotFound(new { success = false, message = "Bildirim bulunamadı." });
        if (notification.AppUserId != userId) return Unauthorized();

        notification.IsRead = true;
        await _notificationWrite.SaveAsync();

        return Ok(new { success = true });
    }

    [HttpPatch("mark-all-as-read")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString)) return Unauthorized();

        var userId = Guid.Parse(userIdString);

        var unreadNotifications = await _notificationRead.GetAll(true)
            .Where(n => n.AppUserId == userId && !n.IsRead)
            .ToListAsync();

        foreach (var notification in unreadNotifications)
        {
            notification.IsRead = true;
        }

        if (unreadNotifications.Any())
        {
            await _notificationWrite.SaveAsync();
        }

        return Ok(new { success = true });
    }
}
