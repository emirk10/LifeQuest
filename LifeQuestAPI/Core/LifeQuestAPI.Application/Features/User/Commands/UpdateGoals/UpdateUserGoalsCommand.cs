using LifeQuestAPI.Application.Abstractions.Notifications;
using LifeQuestAPI.Application.Repositories;
using MediatR;

namespace LifeQuestAPI.Application.Features.User.Commands.UpdateGoals;



public class UpdateUserGoalsCommandRequest : IRequest<UpdateUserGoalsCommandResponse>
{
    public Guid UserId { get; set; }
    public List<string> NewGoals { get; set; } = new();
    public List<string> RemovedGoals { get; set; } = new();
}

public class UpdateUserGoalsCommandResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string> UpdatedGoals { get; set; } = new();
}



public sealed class UpdateUserGoalsCommandHandler
    : IRequestHandler<UpdateUserGoalsCommandRequest, UpdateUserGoalsCommandResponse>
{
    private readonly IAppUserReadRepository _userRead;
    private readonly IAppUserWriteRepository _userWrite;
    private readonly IAppNotificationService _notifService;

    public UpdateUserGoalsCommandHandler(
        IAppUserReadRepository userRead,
        IAppUserWriteRepository userWrite,
        IAppNotificationService notifService)
    {
        _userRead = userRead;
        _userWrite = userWrite;
        _notifService = notifService;
    }

    public async Task<UpdateUserGoalsCommandResponse> Handle(
        UpdateUserGoalsCommandRequest request, CancellationToken cancellationToken)
    {
        var user = await _userRead.GetByIdAsync(request.UserId.ToString(), tracking: true);
        if (user == null)
            return new() { Success = false, Message = "Kullanıcı bulunamadı." };

        var goals = user.UserGoals ?? new List<string>();

        
        foreach (var r in request.RemovedGoals)
            goals.Remove(r);

        
        foreach (var n in request.NewGoals)
            if (!goals.Contains(n, StringComparer.OrdinalIgnoreCase))
                goals.Add(n);

        user.UserGoals = goals;
        user.UpdatedAt = DateTime.UtcNow;

        _userWrite.Update(user);
        await _userWrite.SaveAsync();

        
        await _notifService.SendNotificationAsync(
            user.Id,
            "Hedeflerin güncellendi! 🎯",
            "Yeni hedeflerine uygun görevler hazırlanıyor.",
            "SystemMessage");

        return new()
        {
            Success = true,
            Message = "Hedeflerin başarıyla güncellendi.",
            UpdatedGoals = goals
        };
    }
}
