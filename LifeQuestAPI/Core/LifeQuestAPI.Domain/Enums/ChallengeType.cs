namespace LifeQuestAPI.Domain.Enums;

/// <summary>SP-05: Mücadele tipi.</summary>
public enum ChallengeType
{
    /// <summary>Belirli sürede en çok görevi tamamla.</summary>
    TaskCount = 0,

    /// <summary>Belirli sürede en çok XP kazan.</summary>
    XPRace = 1,

    /// <summary>Seriyi kim daha uzun tutar.</summary>
    StreakBattle = 2,

    /// <summary>Belirli kategoride en çok görev.</summary>
    CategoryFocus = 3,

    /// <summary>Aynı görevi kim önce tamamlar.</summary>
    SpecificTask = 4
}
