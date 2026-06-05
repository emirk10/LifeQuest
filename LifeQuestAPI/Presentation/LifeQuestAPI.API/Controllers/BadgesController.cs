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

    [HttpGet("getall")]
    public async Task<IActionResult> GetAllBadges()
    {
        var response = await _mediator.Send(new LifeQuestAPI.Application.Features.Badges.Queries.GetAll.GetAllBadgesQueryRequest());
        return Ok(response);
    }

    [HttpPost("create-badge")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateBadge(CreateBadgeCommandRequest request)
    {
        var response = await _mediator.Send(request);
        return Ok(response);
    }

    [HttpPost("create-bulk-badges")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateBulkBadges([FromBody] CreateBulkBadgesCommandRequest request)
    {
        var response = await _mediator.Send(request);
        return Ok(response);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateBadge([FromRoute] Guid id, [FromBody] LifeQuestAPI.Application.Features.Badges.Commands.UpdateBadge.UpdateBadgeCommandRequest request)
    {
        if (id != request.Id) return BadRequest(new { success = false, message = "ID eşleşmiyor." });
        var response = await _mediator.Send(request);
        if (!response.IsSuccess) return NotFound(response);
        return Ok(response);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteBadge([FromRoute] Guid id)
    {
        var response = await _mediator.Send(new LifeQuestAPI.Application.Features.Badges.Commands.DeleteBadge.DeleteBadgeCommandRequest { Id = id });
        if (!response.IsSuccess) return NotFound(response);
        return Ok(response);
    }
}