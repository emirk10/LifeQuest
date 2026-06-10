using LifeQuestAPI.Application.Repositories;
using MediatR;

namespace LifeQuestAPI.Application.Features.User.Commands.Vacation;



public class StartVacationCommandRequest : IRequest<VacationCommandResponse>
{
    public Guid UserId { get; set; }
    public DateOnly? EndDate { get; set; }
    public string? Reason { get; set; } 
}

public class VacationCommandResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}



public sealed class StartVacationCommandHandler
    : IRequestHandler<StartVacationCommandRequest, VacationCommandResponse>
{
    private readonly IAppUserReadRepository _userRead;
    private readonly IAppUserWriteRepository _userWrite;

    public StartVacationCommandHandler(
        IAppUserReadRepository userRead,
        IAppUserWriteRepository userWrite)
    {
        _userRead = userRead;
        _userWrite = userWrite;
    }

    public async Task<VacationCommandResponse> Handle(
        StartVacationCommandRequest request, CancellationToken cancellationToken)
    {
        var user = await _userRead.GetByIdAsync(request.UserId.ToString(), tracking: true);
        if (user == null)
            return new() { Success = false, Message = "Kullanıcı bulunamadı." };

        if (user.IsOnVacation)
            return new() { Success = false, Message = "Zaten tatil modundasın." };

        user.IsOnVacation      = true;
        user.VacationStartDate = DateOnly.FromDateTime(DateTime.UtcNow);
        user.VacationEndDate   = request.EndDate;
        user.VacationReason    = request.Reason;
        user.FrozenStreak      = 0; 
        user.UpdatedAt         = DateTime.UtcNow;

        _userWrite.Update(user);
        await _userWrite.SaveAsync();

        return new()
        {
            Success = true,
            Message = "Tatil modu başlatıldı! 🏖️ Streak'in korunuyor."
        };
    }
}
