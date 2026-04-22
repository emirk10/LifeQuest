using LifeQuestAPI.Application.DTOs;
using LifeQuestAPI.Application.Repositories;
using LifeQuestAPI.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LifeQuestAPI.Application.Features.UserTasks.Queries.GetUserTasks;

public sealed class GetUserTasksQueryHandler : IRequestHandler<GetUserTasksQueryRequest, GetUserTasksQueryResponse>
{
    private readonly IUserTaskReadRepository _userTaskReadRepository;

    public GetUserTasksQueryHandler(IUserTaskReadRepository userTaskReadRepository)
    {
        _userTaskReadRepository = userTaskReadRepository;
    }

    public async Task<GetUserTasksQueryResponse> Handle(GetUserTasksQueryRequest request, CancellationToken cancellationToken)
    {
        IQueryable<UserTask> query = _userTaskReadRepository
            .GetWhere(ut => ut.AppUserId == request.UserId, tracking: false)
            .Include(ut => ut.AppTask)
                .ThenInclude(t => t.Category);

        if (request.CategoryId.HasValue)
        {
            query = query.Where(ut => ut.AppTask.CategoryId == request.CategoryId.Value);
        }

        var tasks = await query
            .OrderBy(ut => ut.CompletedAt.HasValue)
            .ThenByDescending(ut => ut.AssignedAt)
            .Select(ut => new UserTaskDto
            {
                Id = ut.Id,
                Title = ut.AppTask.Title,
                Description = ut.AppTask.Description,
                Xp = ut.AppTask.BaseXP,
                BaseXP = ut.AppTask.BaseXP,
                Difficulty = ut.AppTask.Difficulty.ToString(),
                IsCompleted = ut.CompletedAt.HasValue,
                AssignedAt = ut.AssignedAt,
                CategoryName = ut.AppTask.Category != null ? ut.AppTask.Category.Name : null
            })
            .ToListAsync(cancellationToken);

        return new GetUserTasksQueryResponse
        {
            Tasks = tasks
        };
    }
}