using LifeQuestAPI.Application.Abstractions;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LifeQuestAPI.Application.Features.Admin.Commands.WipeAndSeed;

public sealed record WipeAndSeedCommandRequest : IRequest<WipeAndSeedCommandResponse>
{
    public bool WipeDatabase { get; set; } = false;
    public List<CategorySeedRequestDto>? Categories { get; set; }
    public List<TaskSeedRequestDto>? Tasks { get; set; }
    public List<BadgeSeedRequestDto>? Badges { get; set; }
    public List<AchievementSeedRequestDto>? Achievements { get; set; }
}

public sealed record WipeAndSeedCommandResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

public sealed class WipeAndSeedCommandHandler : IRequestHandler<WipeAndSeedCommandRequest, WipeAndSeedCommandResponse>
{
    private readonly IDbWipeAndSeedService _dbWipeAndSeedService;

    public WipeAndSeedCommandHandler(IDbWipeAndSeedService dbWipeAndSeedService)
    {
        _dbWipeAndSeedService = dbWipeAndSeedService;
    }

    public async Task<WipeAndSeedCommandResponse> Handle(WipeAndSeedCommandRequest request, CancellationToken cancellationToken)
    {
        var result = await _dbWipeAndSeedService.WipeAndSeedAsync(new WipeAndSeedServiceRequestDto
        {
            WipeDatabase = request.WipeDatabase,
            Categories = request.Categories,
            Tasks = request.Tasks,
            Badges = request.Badges,
            Achievements = request.Achievements
        });

        return new WipeAndSeedCommandResponse
        {
            Success = result.Success,
            Message = result.Message
        };
    }
}
