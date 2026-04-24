using LifeQuestAPI.Application.DTOs;
using LifeQuestAPI.Domain.Enums;

namespace LifeQuestAPI.Application.Abstractions;

public interface ITaskRecommendationService
{
    Task<List<TaskRecommendationDto>> GetContextualRecommendationsAsync(
        Guid userId,
        MoodMode mood,
        DayTimeSlot timeSlot,
        int energyLevel,
        int availableMinutes,
        int count = 6);
}
