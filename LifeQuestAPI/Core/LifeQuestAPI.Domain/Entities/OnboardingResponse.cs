using LifeQuestAPI.Domain.Entities.Common;
using LifeQuestAPI.Domain.Entities.Identity;

namespace LifeQuestAPI.Domain.Entities;

/// <summary>SP-05: Onboarding sırasında verilen ham cevapları saklar.</summary>
public sealed class OnboardingResponse : BaseEntity
{
    public Guid AppUserId { get; set; }
    public AppUser AppUser { get; set; } = default!;

    /// <summary>Soru kodu: wakeup_time, lifestyle, motivation_killer vb.</summary>
    public string QuestionKey { get; set; } = string.Empty;

    /// <summary>Ham cevap JSON (string, array veya number olabilir).</summary>
    public string Answer { get; set; } = string.Empty;

    public DateTime AnsweredAt { get; set; } = DateTime.UtcNow;
}
