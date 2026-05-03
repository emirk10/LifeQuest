using MediatR;
using System;

namespace LifeQuestAPI.Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenCommandRequest : IRequest<RefreshTokenCommandResponse>
{
    public string RefreshToken { get; set; } = string.Empty;
}

public class RefreshTokenCommandResponse
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Token { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiration { get; set; }
}
