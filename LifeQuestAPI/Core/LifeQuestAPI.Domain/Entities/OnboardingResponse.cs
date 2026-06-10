using LifeQuestAPI.Domain.Entities.Common;
using LifeQuestAPI.Domain.Entities.Identity;

namespace LifeQuestAPI.Domain.Entities;


public sealed class OnboardingResponse : BaseEntity
{
    public Guid AppUserId { get; set; }
    public AppUser AppUser { get; set; } = default!;

    
    public string QuestionKey { get; set; } = string.Empty;

    
    public string Answer { get; set; } = string.Empty;

    public DateTime AnsweredAt { get; set; } = DateTime.UtcNow;
}
