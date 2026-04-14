using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LifeQuestAPI.Application.Repositories;
using LifeQuestAPI.Domain.Entities;
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
                                             .ToListAsync(cancellationToken);

        var response = tasks.Select(t => new GetAllTasksQueryResponse
        {
            Id = t.Id,
            Title = t.Title,
            Description = t.Description,
            CategoryName = t.Category != null ? t.Category.Name : "Kategorisiz",
            Difficulty = t.Difficulty.ToString(),
            BaseXP = t.BaseXP,
            MinLevel = t.MinLevel,
            VerificationType = t.VerificationType.ToString(),
            MinReflectionWords = t.MinReflectionWords,
            MinTimerMinutes = t.MinTimerMinutes,
            VerificationHint = t.VerificationHint,
            ExpectedImageCategory = t.ExpectedImageCategory
        }).ToList();

        return response;
    }
}