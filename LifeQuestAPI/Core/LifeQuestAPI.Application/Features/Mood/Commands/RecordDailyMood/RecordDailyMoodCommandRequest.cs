using LifeQuestAPI.Application.DTOs;
using LifeQuestAPI.Domain.Enums;
using MediatR;

namespace LifeQuestAPI.Application.Features.Mood.Commands.RecordDailyMood;

public class RecordDailyMoodCommandRequest : IRequest<UserDailyMoodDto>
{
    public Guid UserId { get; set; }
    public MoodMode MoodMode { get; set; }
    public int AvailableMinutes { get; set; }
    public int EnergyLevel { get; set; }
}
