using System;
using System.Collections.Generic;
using LifeQuestAPI.Domain.Enums;
using MediatR;

namespace LifeQuestAPI.Application.Features.Goals.Queries.GetUserGoals;

public class GetUserGoalsQueryRequest : IRequest<GetUserGoalsQueryResponse>
{
    public Guid UserId { get; set; }
}

public class GetUserGoalsQueryResponse
{
    public bool Success { get; set; }
    public List<GoalDto> Goals { get; set; } = new();
}

public class GoalDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public GoalType Type { get; set; }
    public int TargetValue { get; set; }
    public int CurrentValue { get; set; }
    public string? CategoryName { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsCompleted { get; set; }
    public int BonusXP { get; set; }
}
