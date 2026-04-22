using MediatR;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using LifeQuestAPI.Application.Features.UserTasks.Queries.GetUserTasks;
using LifeQuestAPI.Application.Features.UserTasks.Commands.CompleteUserTask;
using LifeQuestAPI.Application.Features.UserTasks.Commands.RequestNewQuests;

namespace LifeQuestAPI.API.Controllers;

[Route("api/user-tasks")]
[ApiController]
[Authorize]
public class UserTasksController : ControllerBase
{
    private readonly IMediator _mediator;

    public UserTasksController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetUserTasks([FromQuery] Guid? categoryId)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString)) return Unauthorized();

        var request = new GetUserTasksQueryRequest
        {
            UserId = Guid.Parse(userIdString),
            CategoryId = categoryId
        };

        var response = await _mediator.Send(request);
        return Ok(response);
    }

    [HttpPatch("{id}/complete")]
    public async Task<IActionResult> CompleteTask([FromRoute] Guid id)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString)) return Unauthorized();

        var request = new CompleteUserTaskCommandRequest
        {
            UserTaskId = id,
            UserId = Guid.Parse(userIdString)
        };

        var response = await _mediator.Send(request);

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    [HttpPost("request-new")]
    public async Task<IActionResult> RequestNewQuests()
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString)) return Unauthorized();

        var request = new RequestNewQuestsCommandRequest { UserId = Guid.Parse(userIdString) };
        var response = await _mediator.Send(request);

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    [HttpPost("assign/{appTaskId}")]
    public async Task<IActionResult> AssignSpecificTask([FromRoute] Guid appTaskId)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString)) return Unauthorized();

        var request = new LifeQuestAPI.Application.Features.UserTasks.Commands.AssignSpecificTask.AssignSpecificTaskCommandRequest
        {
            UserId = Guid.Parse(userIdString),
            AppTaskId = appTaskId
        };

        var response = await _mediator.Send(request);

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }
}