using LifeQuestAPI.Application.Abstractions.AI;
using LifeQuestAPI.Application.Repositories;
using LifeQuestAPI.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace LifeQuestAPI.Application.Features.Chat.Commands.SendMessage;

public sealed class SendMessageCommandHandler
    : IRequestHandler<SendMessageCommandRequest, SendMessageCommandResponse>
{
    private readonly IAIChatService _aiChatService;
    private readonly IUserTaskReadRepository _userTaskReadRepository;
    private readonly IAppTaskReadRepository _appTaskReadRepository;
    private readonly IAppUserReadRepository _userReadRepository;

    public SendMessageCommandHandler(
        IAIChatService aiChatService,
        IUserTaskReadRepository userTaskReadRepository,
        IAppTaskReadRepository appTaskReadRepository,
        IAppUserReadRepository userReadRepository)
    {
        _aiChatService = aiChatService;
        _userTaskReadRepository = userTaskReadRepository;
        _appTaskReadRepository = appTaskReadRepository;
        _userReadRepository = userReadRepository;
    }

    public async Task<SendMessageCommandResponse> Handle(
        SendMessageCommandRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            return new SendMessageCommandResponse
            {
                Success = false,
                Response = "Mesaj bos olamaz."
            };
        }

        var result = await _aiChatService.ProcessMessageAsync(request.UserId, request.Message);
        
        var response = new SendMessageCommandResponse
        {
            Success = true,
            Intent = result.Intent,
            Confidence = result.Confidence,
            Response = result.Response,
            Sentiment = result.Sentiment,
            SentimentConfidence = result.SentimentConfidence,
            Suggestions = result.Suggestions
        };

        // RAG Logic Based on Intent
        if (result.Intent == "gorev_sorgula")
        {
            var activeTasks = await _userTaskReadRepository.Table
                .Include(ut => ut.AppTask)
                .Where(ut => ut.AppUserId == request.UserId && ut.Status == AppTaskStatus.Assigned)
                .ToListAsync(cancellationToken);

            if (activeTasks.Any())
            {
                var sb = new StringBuilder();
                sb.AppendLine($"Bugün tamamlanmayı bekleyen {activeTasks.Count} aktif görevin var:");
                foreach(var task in activeTasks)
                {
                    sb.AppendLine($"- {task.AppTask.Title} (+{task.AppTask.BaseXP} XP)");
                }
                sb.AppendLine("\nGörev sayfana giderek bunları bitirebilirsin. 💪");
                response.Response = sb.ToString();
            }
            else
            {
                response.Response = "Şu anda hiç aktif görevin yok. 'Yeni görev ver' diyerek benden hemen bir tane isteyebilirsin!";
            }
        }
        else if (result.Intent == "oneri")
        {
            var user = await _userReadRepository.GetByIdAsync(request.UserId.ToString());
            
            // Get already assigned or completed tasks
            var userTaskIds = await _userTaskReadRepository.Table
                .Where(ut => ut.AppUserId == request.UserId)
                .Select(ut => ut.AppTaskId)
                .ToListAsync(cancellationToken);

            // Find an appropriate unassigned task randomly
            var suggestedTask = await _appTaskReadRepository.Table
                .Where(t => t.IsActive && t.MinLevel <= (user != null ? user.GeneralLevel : 1) && !userTaskIds.Contains(t.Id))
                .OrderBy(r => Guid.NewGuid())
                .FirstOrDefaultAsync(cancellationToken);

            if (suggestedTask != null)
            {
                response.Response = $"Sana çok güzel bir görev buldum: '{suggestedTask.Title}'\n\nBu görev seviyene çok uygun. Aşağıdaki butona tıklayarak hemen listene ekleyebilirsin!";
                response.SuggestedTaskId = suggestedTask.Id;
                response.SuggestedTaskTitle = suggestedTask.Title;
                response.SuggestedTaskXp = suggestedTask.BaseXP;
                response.SuggestedTaskDifficulty = suggestedTask.Difficulty.ToString();
            }
            else
            {
                response.Response = "Şu an için sana önerebileceğim yeni bir görev bulamadım, maşallah tüm açık görevleri silip süpürmüşsün!";
            }
        }

        return response;
    }
}
