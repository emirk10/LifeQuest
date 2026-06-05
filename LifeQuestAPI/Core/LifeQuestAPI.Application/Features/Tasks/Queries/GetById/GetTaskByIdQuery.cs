using LifeQuestAPI.Application.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LifeQuestAPI.Application.Features.Tasks.Queries.GetById;

public sealed record GetTaskByIdQueryRequest : IRequest<GetTaskByIdQueryResponse?>
{
    public Guid Id { get; set; }
}

public sealed record GetTaskByIdQueryResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public string Difficulty { get; set; } = string.Empty;
    public int BaseXP { get; set; }
    public int MinLevel { get; set; }
    public int MaxLevel { get; set; }
    public bool IsDaily { get; set; }
    public bool IsActive { get; set; }
}

public sealed class GetTaskByIdQueryHandler : IRequestHandler<GetTaskByIdQueryRequest, GetTaskByIdQueryResponse?>
{
    private readonly IAppTaskReadRepository _taskReadRepository;

    public GetTaskByIdQueryHandler(IAppTaskReadRepository taskReadRepository)
    {
        _taskReadRepository = taskReadRepository;
    }

    public async Task<GetTaskByIdQueryResponse?> Handle(GetTaskByIdQueryRequest request, CancellationToken cancellationToken)
    {
        var task = await _taskReadRepository
            .GetWhere(t => t.Id == request.Id, tracking: false)
            .Include(t => t.Category)
            .FirstOrDefaultAsync(cancellationToken);

        if (task == null) return null;

        return new GetTaskByIdQueryResponse
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            CategoryName = task.Category != null ? task.Category.Name : string.Empty,
            CategoryId = task.CategoryId,
            Difficulty = task.Difficulty.ToString(),
            BaseXP = task.BaseXP,
            MinLevel = task.MinLevel,
            MaxLevel = task.MaxLevel,
            IsDaily = task.IsDaily,
            IsActive = task.IsActive
        };
    }
}
