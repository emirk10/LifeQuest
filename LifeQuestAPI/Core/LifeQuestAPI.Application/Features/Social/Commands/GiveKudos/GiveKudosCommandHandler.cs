using LifeQuestAPI.Application.Abstractions.Gamification;
using LifeQuestAPI.Application.Abstractions.Notifications;
using LifeQuestAPI.Application.Repositories;
using LifeQuestAPI.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LifeQuestAPI.Application.Features.Social.Commands.GiveKudos;

public sealed class GiveKudosCommandHandler : IRequestHandler<GiveKudosCommandRequest, GiveKudosCommandResponse>
{
    private readonly IKudosWriteRepository _kudosWriteRepository;
    private readonly IKudosReadRepository _kudosReadRepository;
    private readonly IUserTaskReadRepository _userTaskReadRepository;
    private readonly IPostTagReadRepository _postTagReadRepository;
    private readonly IGamificationService _gamificationService;
    private readonly IAppNotificationService _notificationService;

    public GiveKudosCommandHandler(
        IKudosWriteRepository kudosWriteRepository,
        IKudosReadRepository kudosReadRepository,
        IUserTaskReadRepository userTaskReadRepository,
        IPostTagReadRepository postTagReadRepository,
        IGamificationService gamificationService,
        IAppNotificationService notificationService)
    {
        _kudosWriteRepository = kudosWriteRepository;
        _kudosReadRepository = kudosReadRepository;
        _userTaskReadRepository = userTaskReadRepository;
        _postTagReadRepository = postTagReadRepository;
        _gamificationService = gamificationService;
        _notificationService = notificationService;
    }

    public async Task<GiveKudosCommandResponse> Handle(GiveKudosCommandRequest request, CancellationToken cancellationToken)
    {
        var userTask = await _userTaskReadRepository.Table
            .Include(ut => ut.AppUser)
            .FirstOrDefaultAsync(ut => ut.Id == request.UserTaskId, cancellationToken);

        
        if (userTask == null)
        {
            var postTag = await _postTagReadRepository.Table
                .Include(pt => pt.Post)
                    .ThenInclude(p => p.AppUser)
                .FirstOrDefaultAsync(pt => pt.Id == request.UserTaskId, cancellationToken);

            if (postTag != null)
            {
                
                userTask = await _userTaskReadRepository.Table
                    .Include(ut => ut.AppUser)
                    .OrderByDescending(ut => ut.CompletedAt) 
                    .FirstOrDefaultAsync(ut => ut.AppUserId == postTag.Post.AppUserId && 
                                              ut.AppTaskId == postTag.AppTaskId && 
                                              ut.CompletedAt.HasValue, cancellationToken);
            }
        }

        if (userTask == null)
            return new() { Success = false, Message = "Görev bulunamadı." };

        if (userTask.AppUserId == request.SenderId)
            return new() { Success = false, Message = "Kendi görevinize kudos veremezsiniz." };

        
        var existingKudos = await _kudosReadRepository.Table
            .AnyAsync(k => k.SenderId == request.SenderId && k.UserTaskId == request.UserTaskId, cancellationToken);
        if (existingKudos)
            return new() { Success = false, Message = "Bu görev için zaten kudos verdiniz." };

        
        var today = DateTime.UtcNow.Date;
        var kudosCountToday = await _kudosReadRepository.Table
            .CountAsync(k => k.ReceiverId == userTask.AppUserId && k.CreatedAt >= today, cancellationToken);

        if (kudosCountToday >= 3)
            return new() { Success = false, Message = "Bu kullanıcı bugün için maksimum kudos limitine ulaşmış." };

        var kudos = new Kudos
        {
            SenderId = request.SenderId,
            ReceiverId = userTask.AppUserId,
            UserTaskId = userTask.Id 
        };

        await _kudosWriteRepository.AddAsync(kudos);
        await _kudosWriteRepository.SaveAsync();

        
        await _gamificationService.AddExperienceAsync(userTask.AppUserId, 2);

        
        await _notificationService.SendNotificationAsync(
            userTask.AppUserId,
            "Kudos Aldın! 🌟",
            "Bir arkadaşın başardığın görev için sana kudos gönderdi! +2 XP",
            "KudosReceived");

        return new() { Success = true, Message = "Kudos başarıyla gönderildi! ✨" };
    }
}
