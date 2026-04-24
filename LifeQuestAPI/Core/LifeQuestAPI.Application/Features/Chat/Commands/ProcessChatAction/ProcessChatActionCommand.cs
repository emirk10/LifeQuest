using LifeQuestAPI.Application.DTOs;
using LifeQuestAPI.Application.Features.UserTasks.Commands.PostponeTask;
using LifeQuestAPI.Application.Features.UserTasks.Commands.PauseResumeTask;
using LifeQuestAPI.Application.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LifeQuestAPI.Application.Features.Chat.Commands.ProcessChatAction;

public class ProcessChatActionCommandRequest : IRequest<ChatActionResultDto>
{
    public Guid UserId { get; set; }
    public Guid? MessageId { get; set; }
    public string ActionType { get; set; } = string.Empty;
    public Guid? TaskId { get; set; }
    public Dictionary<string, string>? Payload { get; set; }
}

public sealed class ProcessChatActionCommandHandler
    : IRequestHandler<ProcessChatActionCommandRequest, ChatActionResultDto>
{
    private readonly IMediator _mediator;
    private readonly IChatBotMessageWriteRepository _chatWriteRepository;
    private readonly IChatBotMessageReadRepository _chatReadRepository;

    public ProcessChatActionCommandHandler(
        IMediator mediator,
        IChatBotMessageWriteRepository chatWriteRepository,
        IChatBotMessageReadRepository chatReadRepository)
    {
        _mediator = mediator;
        _chatWriteRepository = chatWriteRepository;
        _chatReadRepository = chatReadRepository;
    }

    public async Task<ChatActionResultDto> Handle(
        ProcessChatActionCommandRequest request, CancellationToken cancellationToken)
    {
        try
        {
            string resultMessage;

            switch (request.ActionType)
            {
                case "postpone_task":
                    if (!request.TaskId.HasValue)
                        return Fail("Görev ID eksik.");

                    var tomorrow = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
                    await _mediator.Send(new PostponeTaskCommandRequest
                    {
                        UserId = request.UserId,
                        UserTaskId = request.TaskId.Value,
                        PostponeTo = tomorrow,
                        SkipReason = "Chatbot üzerinden ertelendi"
                    }, cancellationToken);
                    resultMessage = "✅ Görev yarına ertelendi (%70 XP).";
                    break;

                case "pause_task":
                    if (!request.TaskId.HasValue)
                        return Fail("Görev ID eksik.");
                    await _mediator.Send(new PauseTaskCommandRequest
                    {
                        UserId = request.UserId,
                        UserTaskId = request.TaskId.Value
                    }, cancellationToken);
                    resultMessage = "⏸️ Görev duraklatıldı.";
                    break;

                case "resume_task":
                    if (!request.TaskId.HasValue)
                        return Fail("Görev ID eksik.");
                    await _mediator.Send(new ResumeTaskCommandRequest
                    {
                        UserId = request.UserId,
                        UserTaskId = request.TaskId.Value
                    }, cancellationToken);
                    resultMessage = "▶️ Görev devam ediyor.";
                    break;

                case "activate_light_mode":
                    resultMessage = "💡 Hafif mod aktif! Sana en kolay görevi önerdim.";
                    break;

                default:
                    return Fail($"Bilinmeyen aksiyon: {request.ActionType}");
            }

            // Mesajı güncelle (ActionSelected)
            if (request.MessageId.HasValue)
            {
                var msg = await _chatReadRepository.GetByIdAsync(
                    request.MessageId.Value.ToString(), tracking: true);
                if (msg != null)
                {
                    msg.ActionSelected = request.ActionType;
                    msg.ActionTaken = request.TaskId.HasValue
                        ? $"{request.ActionType}:{request.TaskId}"
                        : request.ActionType;
                    msg.UpdatedAt = DateTime.UtcNow;
                    await _chatWriteRepository.SaveAsync();
                }
            }

            return new ChatActionResultDto
            {
                Success = true,
                Message = resultMessage,
                ActionType = request.ActionType,
                AffectedTaskId = request.TaskId
            };
        }
        catch (Exception ex)
        {
            return Fail($"Aksiyon işlenirken hata: {ex.Message}");
        }
    }

    private static ChatActionResultDto Fail(string msg) => new()
    {
        Success = false,
        Message = msg
    };
}
