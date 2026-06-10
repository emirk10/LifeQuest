using LifeQuestAPI.Application.Repositories;
using LifeQuestAPI.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LifeQuestAPI.Application.Features.Chat.Queries.GetChatHistory;

public class GetChatHistoryQueryRequest : IRequest<List<ChatBotMessageDto>>
{
    public Guid UserId { get; set; }
    public int Limit { get; set; } = 50;
}

public class GetChatHistoryQueryHandler : IRequestHandler<GetChatHistoryQueryRequest, List<ChatBotMessageDto>>
{
    private readonly IChatBotMessageReadRepository _chatReadRepository;

    public GetChatHistoryQueryHandler(IChatBotMessageReadRepository chatReadRepository)
    {
        _chatReadRepository = chatReadRepository;
    }

    public async Task<List<ChatBotMessageDto>> Handle(GetChatHistoryQueryRequest request, CancellationToken cancellationToken)
    {
        var messages = await _chatReadRepository
            .GetWhere(m => m.AppUserId == request.UserId, tracking: false)
            .OrderByDescending(m => m.CreatedAt)
            .Take(request.Limit)
            .ToListAsync(cancellationToken);

        
        return messages.OrderBy(m => m.CreatedAt).Select(m => new ChatBotMessageDto
        {
            Id = m.Id,
            Content = m.Content,
            SenderType = m.SenderType.ToString(),
            Intent = m.Intent,
            Sentiment = m.Sentiment,
            UrgencyLevel = m.UrgencyLevel,
            ActionTaken = m.ActionTaken,
            ActionSelected = m.ActionSelected,
            RelatedTaskId = m.RelatedTaskId,
            IsProactive = m.IsProactive,
            ActionsOffered = m.ActionsOfferedJson,
            CreatedAt = m.CreatedAt
        }).ToList();
    }
}

public class ChatBotMessageDto
{
    public Guid Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public string SenderType { get; set; } = string.Empty;
    public string? Intent { get; set; }
    public string? Sentiment { get; set; }
    public int UrgencyLevel { get; set; }
    public string? ActionTaken { get; set; }
    public string? ActionSelected { get; set; }
    public Guid? RelatedTaskId { get; set; }
    public bool IsProactive { get; set; }
    public List<string>? ActionsOffered { get; set; }
    public DateTime CreatedAt { get; set; }
}
