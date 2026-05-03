using MediatR;

namespace LifeQuestAPI.Application.Features.Auth.Commands.ForgotPassword;

public class ForgotPasswordCommandRequest : IRequest<ForgotPasswordCommandResponse>
{
    public string Email { get; set; } = string.Empty;
}

public class ForgotPasswordCommandResponse
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
}
