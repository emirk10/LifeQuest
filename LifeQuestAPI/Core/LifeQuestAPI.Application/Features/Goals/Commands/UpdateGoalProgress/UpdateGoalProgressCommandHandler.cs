using LifeQuestAPI.Application.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LifeQuestAPI.Application.Features.Goals.Commands.UpdateGoalProgress;

public class UpdateGoalProgressCommandRequest : IRequest<UpdateGoalProgressCommandResponse>
{
    public Guid UserId { get; set; }
    public Guid GoalId { get; set; }
    public int IncrementBy { get; set; } = 1;
    public string? Note { get; set; }
}

public class UpdateGoalProgressCommandResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int CurrentValue { get; set; }
    public int TargetValue { get; set; }
    public bool IsCompleted { get; set; }
    public int? EarnedXp { get; set; }
}

public sealed class UpdateGoalProgressCommandHandler
    : IRequestHandler<UpdateGoalProgressCommandRequest, UpdateGoalProgressCommandResponse>
{
    private readonly IUserGoalReadRepository _goalReadRepository;
    private readonly IUserGoalWriteRepository _goalWriteRepository;
    private readonly IGoalMilestoneReadRepository _milestoneReadRepository;
    private readonly IGoalMilestoneWriteRepository _milestoneWriteRepository;
    private readonly Abstractions.Gamification.IGamificationService _gamificationService;
    private readonly Abstractions.Notifications.IAppNotificationService _notificationService;

    public UpdateGoalProgressCommandHandler(
        IUserGoalReadRepository goalReadRepository,
        IUserGoalWriteRepository goalWriteRepository,
        IGoalMilestoneReadRepository milestoneReadRepository,
        IGoalMilestoneWriteRepository milestoneWriteRepository,
        Abstractions.Gamification.IGamificationService gamificationService,
        Abstractions.Notifications.IAppNotificationService notificationService)
    {
        _goalReadRepository = goalReadRepository;
        _goalWriteRepository = goalWriteRepository;
        _milestoneReadRepository = milestoneReadRepository;
        _milestoneWriteRepository = milestoneWriteRepository;
        _gamificationService = gamificationService;
        _notificationService = notificationService;
    }

    public async Task<UpdateGoalProgressCommandResponse> Handle(
        UpdateGoalProgressCommandRequest request, CancellationToken cancellationToken)
    {
        var goal = await _goalReadRepository
            .GetWhere(g => g.Id == request.GoalId && g.AppUserId == request.UserId, tracking: true)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new Exception("Hedef bulunamadı.");

        if (goal.IsCompleted)
            return new() { Success = false, Message = "Bu hedef zaten tamamlanmış." };

        goal.CurrentValue += request.IncrementBy;

        
        if (goal.CurrentValue >= goal.TargetValue)
        {
            goal.IsCompleted = true;
            goal.CompletedAt = DateTime.UtcNow;

            await _notificationService.SendNotificationAsync(
                request.UserId,
                "Hedef Tamamlandı! 🏆",
                $"Tebrikler! '{goal.Title}' hedefini tamamladın!",
                "GoalCompleted");
        }
        else
        {
            
            var milestones = await _milestoneReadRepository
                .GetWhere(m => m.UserGoalId == goal.Id && !m.IsCompleted, tracking: true)
                .OrderBy(m => m.OrderIndex)
                .ToListAsync(cancellationToken);

            foreach (var ms in milestones.Where(m => goal.CurrentValue >= m.TargetValue))
            {
                ms.IsCompleted = true;
                ms.CompletedAt = DateTime.UtcNow;
                await _notificationService.SendNotificationAsync(
                    request.UserId, "Milestone! 🎯",
                    $"'{ms.Title}' adımını geçtin!", "MilestoneCompleted");
            }
            await _milestoneWriteRepository.SaveAsync();
        }

        _goalWriteRepository.Update(goal);
        await _goalWriteRepository.SaveAsync();

        return new()
        {
            Success = true,
            Message = goal.IsCompleted ? "Hedef tamamlandı! 🎉" : "İlerleme güncellendi.",
            CurrentValue = goal.CurrentValue,
            TargetValue = goal.TargetValue,
            IsCompleted = goal.IsCompleted,
            EarnedXp = 0
        };
    }
}
