namespace LifeQuestAPI.Domain.Enums;

/// <summary>
/// Görev durum enum'u — SP-01 ile genişletildi.
/// Migration notu: eski Assigned=0 → Suggested=0 (otomatik uyumlu),
/// eski Completed=1 → Completed=4 (veri migration'ı gerekli).
/// </summary>
public enum AppTaskStatus
{
    Suggested = 0,    // Sistem tarafından önerildi, kullanıcı henüz seçmedi
    Selected = 1,     // Kullanıcı seçti, henüz başlamadı
    InProgress = 2,   // Timer çalışıyor
    Paused = 3,       // Duraklat
    Completed = 4,    // Tamamlandı
    Postponed = 5,    // Ertelendi
    Compensated = 6,  // Ertelenmiş görev telafi edildi
    Abandoned = 7,    // Vazgeçildi
    Expired = 8       // Süresi doldu
}