using System;
using System.Security.Claims;
using System.Threading.Tasks;
using LifeQuestAPI.Application.Features.Goals.Commands.CreateUserGoal;
using LifeQuestAPI.Application.Features.Goals.Queries.GetUserGoals;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LifeQuestAPI.API.Controllers;

[Route("api/goals")]
[ApiController]
[Authorize]
public class GoalsController : ControllerBase
{
    private readonly IMediator _mediator;

    public GoalsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyGoals()
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString)) return Unauthorized();

        var request = new GetUserGoalsQueryRequest { UserId = Guid.Parse(userIdString) };
        var response = await _mediator.Send(request);

        return Ok(response);
    }

    [HttpPost]
    public async Task<IActionResult> CreateGoal([FromBody] CreateGoalBody body)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString)) return Unauthorized();

        var request = new CreateUserGoalCommandRequest
        {
            UserId = Guid.Parse(userIdString),
            Title = body.Title,
            Description = body.Description,
            Type = body.Type,
            TargetValue = body.TargetValue,
            CategoryId = body.CategoryId,
            StartDate = body.StartDate,
            EndDate = body.EndDate,
            BonusXP = body.BonusXP
        };

        var response = await _mediator.Send(request);
        return Ok(response);
    }
}

public class CreateGoalBody
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public LifeQuestAPI.Domain.Enums.GoalType Type { get; set; }
    public int TargetValue { get; set; }
    public Guid? CategoryId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int BonusXP { get; set; }
}
