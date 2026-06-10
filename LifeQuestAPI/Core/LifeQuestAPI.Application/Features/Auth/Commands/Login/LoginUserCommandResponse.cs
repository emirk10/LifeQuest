namespace LifeQuestAPI.Application.Features.Auth.Commands.Login;

public sealed record LoginUserCommandResponse
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiration { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Role { get; set; } = "User";
    public bool OnboardingCompleted { get; set; }
}