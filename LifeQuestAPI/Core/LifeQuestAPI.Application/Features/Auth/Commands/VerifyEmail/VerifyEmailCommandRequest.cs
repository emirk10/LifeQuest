using MediatR;

namespace LifeQuestAPI.Application.Features.Auth.Commands.VerifyEmail;

public class VerifyEmailCommandRequest : IRequest<VerifyEmailCommandResponse>
{
    public string Token { get; set; } = string.Empty;
}

public class VerifyEmailCommandResponse
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
}
