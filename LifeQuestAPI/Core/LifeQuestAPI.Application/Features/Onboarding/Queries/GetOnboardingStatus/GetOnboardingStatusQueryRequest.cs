using System;
using System.Threading;
using System.Threading.Tasks;
using LifeQuestAPI.Application.Repositories;
using MediatR;

namespace LifeQuestAPI.Application.Features.Onboarding.Queries.GetOnboardingStatus;

public class GetOnboardingStatusQueryRequest : IRequest<GetOnboardingStatusQueryResponse>
{
    public Guid UserId { get; set; }
}

public class GetOnboardingStatusQueryResponse
{
    public bool OnboardingCompleted { get; set; }
    public bool IsEmailVerified { get; set; }
}

public class GetOnboardingStatusQueryHandler : IRequestHandler<GetOnboardingStatusQueryRequest, GetOnboardingStatusQueryResponse>
{
    private readonly IAppUserReadRepository _userRepo;

    public GetOnboardingStatusQueryHandler(IAppUserReadRepository userRepo)
    {
        _userRepo = userRepo;
    }

    public async Task<GetOnboardingStatusQueryResponse> Handle(GetOnboardingStatusQueryRequest request, CancellationToken ct)
    {
        var user = await _userRepo.GetByIdAsync(request.UserId.ToString());
        if (user == null) throw new Exception("Kullanıcı bulunamadı.");

        return new GetOnboardingStatusQueryResponse
        {
            OnboardingCompleted = user.OnboardingCompleted,
            IsEmailVerified = user.IsEmailVerified
        };
    }
}
