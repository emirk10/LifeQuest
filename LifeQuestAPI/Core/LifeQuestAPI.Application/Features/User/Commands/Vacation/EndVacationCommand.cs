using LifeQuestAPI.Application.Abstractions.Notifications;
using LifeQuestAPI.Application.Repositories;
using MediatR;

namespace LifeQuestAPI.Application.Features.User.Commands.Vacation;



public class EndVacationCommandRequest : IRequest<VacationCommandResponse>
{
    public Guid UserId { get; set; }
}



public sealed class EndVacationCommandHandler
    : IRequestHandler<EndVacationCommandRequest, VacationCommandResponse>
{
    private readonly IAppUserReadRepository _userRead;
    private readonly IAppUserWriteRepository _userWrite;
    private readonly IAppNotificationService _notifService;

    public EndVacationCommandHandler(
        IAppUserReadRepository userRead,
        IAppUserWriteRepository userWrite,
        IAppNotificationService notifService)
    {
        _userRead = userRead;
        _userWrite = userWrite;
        _notifService = notifService;
    }

    public async Task<VacationCommandResponse> Handle(
        EndVacationCommandRequest request, CancellationToken cancellationToken)
    {
        var user = await _userRead.GetByIdAsync(request.UserId.ToString(), tracking: true);
        if (user == null)
            return new() { Success = false, Message = "Kullanıcı bulunamadı." };

        if (!user.IsOnVacation)
            return new() { Success = false, Message = "Tatil modunda değilsin." };

        int restoredStreak = user.FrozenStreak;

        user.IsOnVacation      = false;
        user.VacationEndDate   = DateOnly.FromDateTime(DateTime.UtcNow);
        user.FrozenStreak      = 0;
        user.UpdatedAt         = DateTime.UtcNow;

        _userWrite.Update(user);
        await _userWrite.SaveAsync();

        
        await _notifService.SendNotificationAsync(
            user.Id,
            "Hoş geldin! 🎉",
            $"Tatilden döndün! Streak'in ({restoredStreak} gün) hâlâ yerinde. Hazır mısın?",
            "SystemMessage");

        return new()
        {
            Success = true,
            Message = $"Tatil modu kapatıldı. Streak'in ({restoredStreak} gün) korundu! 🔥"
        };
    }
}
