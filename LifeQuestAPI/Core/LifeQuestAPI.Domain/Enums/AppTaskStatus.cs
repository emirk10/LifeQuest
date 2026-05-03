namespace LifeQuestAPI.Domain.Enums;

/// <summary>
/// Görev durum enum'u — SP-01 ile genişletildi.
/// Migration notu: eski Assigned=0 → Suggested=0 (otomatik uyumlu),
/// eski Completed=1 → Completed=4 (veri migration'ı gerekli).
/// </summary>
public enum AppTaskStatus
{
    Pending = 0,      // Bekliyor
    InProgress = 1,   // Timer çalışıyor
    Completed = 2,    // Tamamlandı
    Failed = 3,       // Başarısız
    Expired = 4,      // Süresi doldu
    Suggested = 5,    // Sistem tarafından önerildi, kullanıcı henüz seçmedi
    Selected = 6,     // Kullanıcı seçti, henüz başlamadı
    Paused = 7,       // Duraklat
    Postponed = 8,    // Ertelendi
    Compensated = 9,  // Ertelenmiş görev telafi edildi
    Abandoned = 10    // Vazgeçildi
}