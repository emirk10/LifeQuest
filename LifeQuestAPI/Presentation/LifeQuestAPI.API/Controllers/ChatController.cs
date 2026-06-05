using LifeQuestAPI.Application.Features.Chat.Commands.ProcessChatAction;
using LifeQuestAPI.Application.Features.Chat.Commands.SendMessage;
using LifeQuestAPI.Application.Features.Chat.Queries.GetChatHistory;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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

    /// <summary>AI chatbot'a mesaj gönder ve bağlam-duyarlı cevap al.</summary>
    [HttpPost("send")]
    public async Task<IActionResult> SendMessage([FromBody] ChatMessageBody body)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString)) return Unauthorized();

        var response = await _mediator.Send(new SendMessageCommandRequest
        {
            UserId = Guid.Parse(userIdString),
            Message = body.Message,
            SaveToHistory = body.SaveToHistory
        });

        if (!response.Success) return BadRequest(response);
        return Ok(response);
    }

    /// <summary>SP-02: Chatbot aksiyon butonuna tıklandığında aksiyonu işle.</summary>
    [HttpPost("action")]
    public async Task<IActionResult> ProcessAction([FromBody] ProcessChatActionBody body)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString)) return Unauthorized();

        var result = await _mediator.Send(new ProcessChatActionCommandRequest
        {
            UserId = Guid.Parse(userIdString),
            MessageId = body.MessageId,
            ActionType = body.ActionType,
            TaskId = body.TaskId,
            Payload = body.Payload
        });

        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    /// <summary>SP-02: Konuşma geçmişi (son 50 mesaj).</summary>
    [HttpGet("history")]
    public async Task<IActionResult> GetHistory([FromQuery] int limit = 50)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString)) return Unauthorized();

        var response = await _mediator.Send(new GetChatHistoryQueryRequest
        {
            UserId = Guid.Parse(userIdString),
            Limit = limit
        });

        return Ok(response);
    }
}

public class ChatMessageBody
{
    public string Message { get; set; } = string.Empty;
    public bool SaveToHistory { get; set; } = true;
}

public class ProcessChatActionBody
{
    public Guid? MessageId { get; set; }
    public string ActionType { get; set; } = string.Empty;
    public Guid? TaskId { get; set; }
    public Dictionary<string, string>? Payload { get; set; }
}
