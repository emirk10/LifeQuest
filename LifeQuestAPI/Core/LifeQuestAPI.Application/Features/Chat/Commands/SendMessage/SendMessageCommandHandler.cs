using LifeQuestAPI.Application.Abstractions.AI;
using LifeQuestAPI.Application.DTOs;
using LifeQuestAPI.Application.Repositories;
using LifeQuestAPI.Domain.Entities;
using LifeQuestAPI.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LifeQuestAPI.Application.Features.Chat.Commands.SendMessage;

public sealed class SendMessageCommandHandler
    : IRequestHandler<SendMessageCommandRequest, SendMessageCommandResponse>
{
    private readonly IAIChatService _aiChatService;
    private readonly IUserTaskReadRepository _userTaskReadRepository;
    private readonly IAppTaskReadRepository _appTaskReadRepository;
    private readonly IAppUserReadRepository _userReadRepository;
    private readonly IUserDailyMoodReadRepository _moodReadRepository;
    private readonly IChatBotMessageWriteRepository _chatWriteRepository;

    public SendMessageCommandHandler(
        IAIChatService aiChatService,
        IUserTaskReadRepository userTaskReadRepository,
        IAppTaskReadRepository appTaskReadRepository,
        IAppUserReadRepository userReadRepository,
        IUserDailyMoodReadRepository moodReadRepository,
        IChatBotMessageWriteRepository chatWriteRepository)
    {
        _aiChatService = aiChatService;
        _userTaskReadRepository = userTaskReadRepository;
        _appTaskReadRepository = appTaskReadRepository;
        _userReadRepository = userReadRepository;
        _moodReadRepository = moodReadRepository;
        _chatWriteRepository = chatWriteRepository;
    }

    public async Task<SendMessageCommandResponse> Handle(
        SendMessageCommandRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
            return new SendMessageCommandResponse { Success = false, Response = "Mesaj boş olamaz." };

        
        var user = await _userReadRepository.GetByIdAsync(request.UserId.ToString());
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var todayTasks = await _userTaskReadRepository.Table
            .Include(ut => ut.AppTask).ThenInclude(t => t!.Category)
            .Where(ut => ut.AppUserId == request.UserId
                && (ut.Status == AppTaskStatus.Selected
                    || ut.Status == AppTaskStatus.InProgress
                    || ut.Status == AppTaskStatus.Paused))
            .ToListAsync(cancellationToken);

        var completedToday = await _userTaskReadRepository.Table
            .CountAsync(ut => ut.AppUserId == request.UserId
                && ut.Status == AppTaskStatus.Completed
                && ut.CompletedAt.HasValue
                && DateOnly.FromDateTime(ut.CompletedAt.Value) == today, cancellationToken);

        var todayMood = await _moodReadRepository
            .GetWhere(m => m.AppUserId == request.UserId && m.Date == today, tracking: false)
            .FirstOrDefaultAsync(cancellationToken);

        var context = new ChatContextDto
        {
            UserName = user?.Name ?? "Kullanıcı",
            Personality = user?.ChatPersonality ?? "friendly",
            TodayTasks = todayTasks.Select(ut => new TaskContextDto
            {
                Id = ut.Id,
                Title = ut.AppTask?.Title ?? string.Empty,
                Status = ut.Status.ToString(),
                Category = ut.AppTask?.Category?.Name ?? string.Empty,
                EstimatedMinutes = ut.AppTask?.EstimatedMinutes
            }).ToList(),
            CompletedToday = completedToday,
            CurrentStreak = user?.CurrentStreak ?? 0,
            TotalXP = user?.TotalXP ?? 0,
            TodayMood = todayMood?.MoodMode.ToString(),
            AvailableMinutes = todayMood?.AvailableMinutes ?? 30,
            GeneralLevel = user?.GeneralLevel ?? 1.0,
            MostSkippedCategory = user?.MostSkippedCategory,
            PreferredMorningTime = user?.PreferredMorningTime?.ToString("HH:mm"),
            DifficultyPreference = user?.DifficultyPreference,
            Strengths = user?.Strengths ?? new(),
            Weaknesses = user?.Weaknesses ?? new(),
            MotivationFactor = user?.MotivationFactor,
            LifestyleType = user?.LifestyleType,
            PrimaryFocusArea = user?.PrimaryFocusArea
        };

        
        var aiResult = await _aiChatService.ProcessMessageWithContextAsync(
            request.UserId, request.Message, context);

        
        if (request.SaveToHistory)
        {
            var chatMessage = new ChatBotMessage
            {
                Id = Guid.NewGuid(),
                AppUserId = request.UserId,
                Content = request.Message,
                SenderType = SenderType.User,
                Intent = aiResult.Intent,
                IntentConfidence = aiResult.IntentConfidence,
                Sentiment = aiResult.Sentiment,
                SentimentConfidence = aiResult.SentimentConfidence,
                UrgencyLevel = aiResult.UrgencyLevel,
                ActionsOfferedJson = aiResult.Actions.Select(a => a.Type).ToList(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _chatWriteRepository.AddAsync(chatMessage);
            await _chatWriteRepository.SaveAsync();
        }

        
        if (aiResult.Intent is "oneri" or "oneri_iste" && (aiResult.SuggestedTasks == null || !aiResult.SuggestedTasks.Any()))
        {
            var userTaskIds = todayTasks.Select(ut => ut.AppTaskId).ToList();
            var userLevel = user != null ? user.GeneralLevel : 1.0;
            var suggestedTask = await _appTaskReadRepository.Table
                .Where(t => t.IsActive && t.MinLevel <= userLevel && !userTaskIds.Contains(t.Id))
                .OrderBy(_ => Guid.NewGuid())
                .FirstOrDefaultAsync(cancellationToken);

            if (suggestedTask != null)
            {
                var responseText = $"Sana çok güzel bir görev buldum: '{suggestedTask.Title}'\n\nBu görev seviyene çok uygun. Aşağıdaki butona tıklayarak hemen listene ekleyebilirsin!";
                
                if (request.SaveToHistory)
                {
                    var botMessage = new ChatBotMessage
                    {
                        Id = Guid.NewGuid(),
                        AppUserId = request.UserId,
                        Content = responseText,
                        SenderType = SenderType.Bot,
                        Intent = aiResult.Intent,
                        IntentConfidence = aiResult.IntentConfidence,
                        Sentiment = aiResult.Sentiment,
                        SentimentConfidence = aiResult.SentimentConfidence,
                        UrgencyLevel = aiResult.UrgencyLevel,
                        ActionsOfferedJson = aiResult.Actions.Select(a => a.Type).ToList(),
                        CreatedAt = DateTime.UtcNow.AddMilliseconds(500),
                        UpdatedAt = DateTime.UtcNow
                    };
                    await _chatWriteRepository.AddAsync(botMessage);
                    await _chatWriteRepository.SaveAsync();
                }

                return new SendMessageCommandResponse
                {
                    Success = true,
                    Intent = aiResult.Intent,
                    Confidence = aiResult.IntentConfidence,
                    Response = responseText,
                    Sentiment = aiResult.Sentiment,
                    SentimentConfidence = aiResult.SentimentConfidence,
                    UrgencyLevel = aiResult.UrgencyLevel,
                    Suggestions = aiResult.Suggestions,
                    Actions = aiResult.Actions,
                    SuggestedTaskId = suggestedTask.Id,
                    SuggestedTaskTitle = suggestedTask.Title,
                    SuggestedTaskXp = suggestedTask.BaseXP,
                    SuggestedTaskDifficulty = suggestedTask.Difficulty.ToString(),
                    FlowRedirect = aiResult.FlowRedirect
                };
            }
        }

        if (request.SaveToHistory)
        {
            var botMsg = new ChatBotMessage
            {
                Id = Guid.NewGuid(),
                AppUserId = request.UserId,
                Content = aiResult.Response,
                SenderType = SenderType.Bot,
                Intent = aiResult.Intent,
                IntentConfidence = aiResult.IntentConfidence,
                Sentiment = aiResult.Sentiment,
                SentimentConfidence = aiResult.SentimentConfidence,
                UrgencyLevel = aiResult.UrgencyLevel,
                ActionsOfferedJson = aiResult.Actions.Select(a => a.Type).ToList(),
                CreatedAt = DateTime.UtcNow.AddMilliseconds(500),
                UpdatedAt = DateTime.UtcNow
            };
            await _chatWriteRepository.AddAsync(botMsg);
            await _chatWriteRepository.SaveAsync();
        }

        return new SendMessageCommandResponse
        {
            Success = true,
            Intent = aiResult.Intent,
            Confidence = aiResult.IntentConfidence,
            Response = aiResult.Response,
            Sentiment = aiResult.Sentiment,
            SentimentConfidence = aiResult.SentimentConfidence,
            UrgencyLevel = aiResult.UrgencyLevel,
            Suggestions = aiResult.Suggestions,
            Actions = aiResult.Actions,
            SuggestedTasks = aiResult.SuggestedTasks,
            ShouldShowMoodSelector = aiResult.ShouldShowMoodSelector,
            FlowRedirect = aiResult.FlowRedirect
        };
    }
}
