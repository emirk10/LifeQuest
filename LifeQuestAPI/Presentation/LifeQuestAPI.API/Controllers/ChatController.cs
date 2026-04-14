using MediatR;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using LifeQuestAPI.Application.Features.Chat.Commands.SendMessage;

namespace LifeQuestAPI.API.Controllers;

[Route("api/chat")]
[ApiController]
[Authorize]
public class ChatController : ControllerBase
{
    private readonly IMediator _mediator;

    public ChatController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// AI chatbot'a mesaj gonder ve cevap al.
    /// </summary>
    [HttpPost("send")]
    public async Task<IActionResult> SendMessage([FromBody] ChatMessageBody body)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString)) return Unauthorized();

        var request = new SendMessageCommandRequest
        {
            UserId = Guid.Parse(userIdString),
            Message = body.Message
        };

        var response = await _mediator.Send(request);

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }
}

public class ChatMessageBody
{
    public string Message { get; set; } = string.Empty;
}
