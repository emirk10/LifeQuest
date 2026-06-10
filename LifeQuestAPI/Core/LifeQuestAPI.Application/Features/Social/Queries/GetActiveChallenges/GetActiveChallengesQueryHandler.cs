using LifeQuestAPI.Application.Repositories;
using LifeQuestAPI.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LifeQuestAPI.Application.Features.Social.Queries.GetActiveChallenges;

public class GetActiveChallengesQueryRequest : IRequest<List<ChallengeDto>>
{
    public Guid UserId { get; set; }
    
    public string StatusFilter { get; set; } = "active";
}

public class ChallengeDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string ChallengeType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int MyProgress { get; set; }
    public int OpponentProgress { get; set; }
    public int? TargetValue { get; set; }
    public Guid OpponentId { get; set; }
    public string OpponentName { get; set; } = string.Empty;
    public int? OpponentAvatarId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int DaysRemaining { get; set; }
    public bool IAmChallenger { get; set; }
    public int WinnerXPBonus { get; set; }
    public string? ChallengerMessage { get; set; }
    public Guid? WinnerId { get; set; }
}

public sealed class GetActiveChallengesQueryHandler
    : IRequestHandler<GetActiveChallengesQueryRequest, List<ChallengeDto>>
{
    private readonly IChallengeReadRepository _challengeReadRepository;

    public GetActiveChallengesQueryHandler(IChallengeReadRepository challengeReadRepository)
        => _challengeReadRepository = challengeReadRepository;

    public async Task<List<ChallengeDto>> Handle(
        GetActiveChallengesQueryRequest request, CancellationToken cancellationToken)
    {
        IQueryable<LifeQuestAPI.Domain.Entities.Challenge> query = _challengeReadRepository.GetWhere(
            c => c.ChallengerId == request.UserId || c.OpponentId == request.UserId,
            tracking: false)
            .Include(c => c.Challenger)
            .Include(c => c.Opponent);

        query = request.StatusFilter switch
        {
            "invited" => query.Where(c => c.Status == ChallengeStatus.Invited),
            "completed" => query.Where(c => c.Status == ChallengeStatus.Completed),
            "all" => query,
            _ => query.Where(c => c.Status == ChallengeStatus.Active)
        };

        var challenges = await query.OrderByDescending(c => c.InvitedAt).ToListAsync(cancellationToken);
        var now = DateTime.UtcNow;

        return challenges.Select(c =>
        {
            bool iAmChallenger = c.ChallengerId == request.UserId;
            var opponent = iAmChallenger ? c.Opponent : c.Challenger;

            return new ChallengeDto
            {
                Id = c.Id,
                Title = c.Title,
                Description = c.Description,
                ChallengeType = c.ChallengeType.ToString(),
                Status = c.Status.ToString(),
                MyProgress = iAmChallenger ? c.ChallengerProgress : c.OpponentProgress,
                OpponentProgress = iAmChallenger ? c.OpponentProgress : c.ChallengerProgress,
                TargetValue = c.TargetValue,
                OpponentId = opponent?.Id ?? Guid.Empty,
                OpponentName = opponent?.FullName ?? "Bilinmiyor",
                OpponentAvatarId = opponent?.AvatarId,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                DaysRemaining = Math.Max(0, (int)(c.EndDate - now).TotalDays),
                IAmChallenger = iAmChallenger,
                WinnerXPBonus = c.WinnerXPBonus,
                ChallengerMessage = c.ChallengerMessage,
                WinnerId = c.WinnerId
            };
        }).ToList();
    }
}
