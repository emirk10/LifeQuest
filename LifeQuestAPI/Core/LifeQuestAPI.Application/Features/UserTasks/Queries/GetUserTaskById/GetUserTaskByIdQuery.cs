using LifeQuestAPI.Application.DTOs;
using LifeQuestAPI.Application.Repositories;
using LifeQuestAPI.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LifeQuestAPI.Application.Features.UserTasks.Queries.GetUserTaskById;

public class GetUserTaskByIdQueryRequest : IRequest<UserTaskDetailDto?>
{
    public Guid UserId { get; set; }
    public Guid UserTaskId { get; set; }
}

public class GetUserTaskByIdQueryHandler : IRequestHandler<GetUserTaskByIdQueryRequest, UserTaskDetailDto?>
{
    private readonly IUserTaskReadRepository _userTaskReadRepository;

    public GetUserTaskByIdQueryHandler(IUserTaskReadRepository userTaskReadRepository)
    {
        _userTaskReadRepository = userTaskReadRepository;
    }

    public async Task<UserTaskDetailDto?> Handle(GetUserTaskByIdQueryRequest request, CancellationToken cancellationToken)
    {
        var ut = await _userTaskReadRepository.Table.AsNoTracking()
            .Include(x => x.AppTask).ThenInclude(t => t.Category)
            .FirstOrDefaultAsync(x => x.Id == request.UserTaskId && x.AppUserId == request.UserId, cancellationToken);

        if (ut == null) return null;

        return new UserTaskDetailDto
        {
            Id = ut.Id,
            AppTaskId = ut.AppTaskId,
            Title = ut.AppTask?.Title ?? string.Empty,
            Description = ut.AppTask?.Description ?? string.Empty,
            CategoryName = ut.AppTask?.Category?.Name ?? string.Empty,
            CategoryIcon = ut.AppTask?.Category?.IconPath ?? "🎯",
            BaseXP = ut.AppTask?.BaseXP ?? 0,
            EarnedXp = ut.EarnedXp,
            Status = ut.Status.ToString(),
            AssignedAt = ut.AssignedAt,
            CompletedAt = ut.CompletedAt,
            DueDate = ut.DueDate,
            ReflectionText = ut.ReflectionText,
            CustomTags = ut.CustomTags
        };
    }
}

public class UserTaskDetailDto
{
    public Guid Id { get; set; }
    public Guid AppTaskId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string CategoryIcon { get; set; } = string.Empty;
    public int BaseXP { get; set; }
    public int EarnedXp { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime AssignedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? DueDate { get; set; }
    public string? ReflectionText { get; set; }
    public List<string>? CustomTags { get; set; }
}
