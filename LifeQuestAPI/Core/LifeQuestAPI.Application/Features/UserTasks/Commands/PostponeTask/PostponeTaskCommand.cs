using LifeQuestAPI.Application.Abstractions;
using LifeQuestAPI.Application.DTOs;
using LifeQuestAPI.Application.Repositories;
using LifeQuestAPI.Domain.Entities;
using LifeQuestAPI.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LifeQuestAPI.Application.Features.UserTasks.Commands.PostponeTask;

public class PostponeTaskCommandRequest : IRequest<PostponeResultDto>
{
    public Guid UserId { get; set; }
    public Guid UserTaskId { get; set; }
    public DateOnly PostponeTo { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
    public string? SkipReason { get; set; }
}

public class PostponeTaskCommandHandler : IRequestHandler<PostponeTaskCommandRequest, PostponeResultDto>
{
    private const int MaxPostponeCount = 3;
    private const decimal PostponeXpRate = 0.70m;

    private readonly IUserTaskReadRepository _userTaskReadRepository;
    private readonly IUserTaskWriteRepository _userTaskWriteRepository;
    private readonly IAppUserReadRepository _userReadRepository;
    private readonly IAppUserWriteRepository _userWriteRepository;
    private readonly IUserBehaviorAnalysisService _behaviorAnalysisService;

    public PostponeTaskCommandHandler(
        IUserTaskReadRepository userTaskReadRepository,
        IUserTaskWriteRepository userTaskWriteRepository,
        IAppUserReadRepository userReadRepository,
        IAppUserWriteRepository userWriteRepository,
        IUserBehaviorAnalysisService behaviorAnalysisService)
    {
        _userTaskReadRepository = userTaskReadRepository;
        _userTaskWriteRepository = userTaskWriteRepository;
        _userReadRepository = userReadRepository;
        _userWriteRepository = userWriteRepository;
        _behaviorAnalysisService = behaviorAnalysisService;
    }

    public async Task<PostponeResultDto> Handle(PostponeTaskCommandRequest request, CancellationToken cancellationToken)
    {
        var userTask = await _userTaskReadRepository
            .GetWhere(ut => ut.Id == request.UserTaskId && ut.AppUserId == request.UserId, tracking: true)
            .Include(ut => ut.AppTask)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new Exception("Görev bulunamadı.");

        if (userTask.PostponeCount >= MaxPostponeCount)
            throw new InvalidOperationException($"Bu görevi artık erteleyemezsin (maks {MaxPostponeCount} erteleme). Vazgeç ya da tamamla.");

        userTask.IsPostponed = true;
        userTask.PostponeCount++;
        userTask.PostponedTo = request.PostponeTo;
        userTask.Status = AppTaskStatus.Postponed;
        userTask.SkipReason = request.SkipReason;
        userTask.XpCompensationRate = PostponeXpRate;

        _userTaskWriteRepository.Update(userTask);

        
        var user = await _userReadRepository.GetByIdAsync(request.UserId.ToString(), tracking: true);
        if (user != null)
        {
            user.TotalPostponeCount++;
            _userWriteRepository.Update(user);
        }

        
        var tomorrowTask = new UserTask
        {
            AppUserId = request.UserId,
            AppTaskId = userTask.AppTaskId,
            Status = AppTaskStatus.Suggested,
            DueDate = DateTime.SpecifyKind(request.PostponeTo.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc),
            IsPostponed = true,
            PostponeCount = userTask.PostponeCount,
            XpCompensationRate = PostponeXpRate
        };
        await _userTaskWriteRepository.AddAsync(tomorrowTask);
        await _userTaskWriteRepository.SaveAsync();

        
        try
        {
            await _behaviorAnalysisService.AnalyzeAndUpdateUserBehaviorAsync(request.UserId, cancellationToken);
        }
        catch
        {
            
        }

        return new PostponeResultDto
        {
            UserTaskId = userTask.Id,
            PostponedTo = request.PostponeTo,
            PostponeCount = userTask.PostponeCount,
            RemainingPostpones = MaxPostponeCount - userTask.PostponeCount,
            XpCompensationRate = PostponeXpRate,
            Message = $"{userTask.AppTask.Title} {request.PostponeTo:dd.MM}'e ertelendi. Tamamlarsan {(int)(userTask.AppTask.BaseXP * PostponeXpRate)} XP kazanacaksın!"
        };
    }
}
