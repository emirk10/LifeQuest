namespace LifeQuestAPI.Application.Features.Auth.Commands.Login;

public sealed record LoginUserCommandResponse
{
    public string Token { get; set; } = string.Empty;
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiration { get; set; }
}