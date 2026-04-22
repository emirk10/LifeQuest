using LifeQuestAPI.Application.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LifeQuestAPI.Application.Features.Tasks.Queries.GetAll;

public sealed class GetAllTasksQueryHandler : IRequestHandler<GetAllTasksQueryRequest, List<GetAllTasksQueryResponse>>
{
    private readonly IAppTaskReadRepository _taskReadRepository;

    public GetAllTasksQueryHandler(IAppTaskReadRepository taskReadRepository)
    {
        _taskReadRepository = taskReadRepository;
    }

    public async Task<List<GetAllTasksQueryResponse>> Handle(GetAllTasksQueryRequest request, CancellationToken cancellationToken)
    {
        var tasks = await _taskReadRepository.GetAll(tracking: false)
                                             .Include(t => t.Category)
                                             .Where(t => t.IsActive)
                                             .OrderBy(t => t.MinLevel)
                                             .ThenBy(t => t.Title)
                                             .ToListAsync(cancellationToken);

        return tasks.Select(t => new GetAllTasksQueryResponse
        {
            Id = t.Id,
            Title = t.Title,
            Description = t.Description,
            CategoryName = t.Category != null ? t.Category.Name : "Kategorisiz",
            CategoryId = t.CategoryId,
            Difficulty = t.Difficulty.ToString(),
            BaseXP = t.BaseXP,
            MinLevel = t.MinLevel,
            MaxLevel = t.MaxLevel,
            IsDaily = t.IsDaily,
            IsActive = t.IsActive
        }).ToList();
    }
}