using LifeQuestAPI.Application.Features.Badges.Commands.CreateBadge;
using LifeQuestAPI.Application.Features.Badges.Commands.CreateBulkBadges;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LifeQuestAPI.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BadgesController : ControllerBase
{
    private readonly IMediator _mediator;

    public BadgesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("create-badge")]
    [Authorize]
    public async Task<IActionResult> CreateBadge(CreateBadgeCommandRequest request)
    {
        var response = await _mediator.Send(request);
        return Ok(response);
    }
    [HttpPost("create-bulk-badges")]
    [Authorize]
    public async Task<IActionResult> CreateBulkBadges([FromBody] CreateBulkBadgesCommandRequest request)
    {
        var response = await _mediator.Send(request);
        return Ok(response);
    }
}