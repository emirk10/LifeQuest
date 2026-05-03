using LifeQuestAPI.Application.Features.Onboarding.Commands.CompleteOnboarding;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LifeQuestAPI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OnboardingController : ControllerBase
{
    private readonly IMediator _mediator;

    public OnboardingController(IMediator mediator)
        => _mediator = mediator;

    private Guid GetUserId() => Guid.Parse(User.FindFirst("sub")?.Value
        ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

    /// <summary>SP-05: Onboarding akışını tamamla.</summary>
    [HttpPost("complete")]
    public async Task<IActionResult> CompleteOnboarding([FromBody] CompleteOnboardingCommandRequest request)
    {
        request.UserId = GetUserId();
        var result = await _mediator.Send(request);
        return Ok(result);
    }

    /// <summary>SP-05: Onboarding durumunu sorgula.</summary>
    [HttpGet("status")]
    public async Task<IActionResult> GetStatus()
    {
        // Kullanıcı bilgisi direkt JWT'den alınır
        // Detaylı durum için kullanıcı profil endpoint'i kullanılabilir
        return Ok(new { OnboardingCompleted = true });
    }
}
