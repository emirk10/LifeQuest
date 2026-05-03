namespace LifeQuestAPI.Domain.Enums;

/// <summary>SP-05: Mücadele durumu.</summary>
public enum ChallengeStatus
{
    /// <summary>Davet gönderildi, kabul bekleniyor.</summary>
    Invited = 0,

    /// <summary>Devam ediyor.</summary>
    Active = 1,

    /// <summary>Tamamlandı.</summary>
    Completed = 2,

    /// <summary>Reddedildi.</summary>
    Declined = 3,

    /// <summary>İptal edildi.</summary>
    Cancelled = 4,

    /// <summary>Süre doldu, kabul edilmeden geçti.</summary>
    Expired = 5
}
