using MediatR;

namespace LifeQuestAPI.Application.Features.Auth.Commands.ResetPassword;

public class ResetPasswordCommandRequest : IRequest<ResetPasswordCommandResponse>
{
    public string Token { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public class ResetPasswordCommandResponse
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
}
