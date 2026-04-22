namespace LifeQuestAPI.Application.Features.User.Queries.GetLeaderboard;

public sealed record GetLeaderboardQueryResponse
{
    public int Rank { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public int TotalXP { get; set; }
    public double GeneralLevel { get; set; }
}