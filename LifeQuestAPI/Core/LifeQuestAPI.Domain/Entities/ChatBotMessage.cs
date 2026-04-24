using LifeQuestAPI.Domain.Entities.Common;
using LifeQuestAPI.Domain.Entities.Identity;
using LifeQuestAPI.Domain.Enums;

namespace LifeQuestAPI.Domain.Entities;

public class ChatBotMessage : BaseEntity
{
    public Guid AppUserId { get; set; }
    public AppUser AppUser { get; set; } = default!;

    public string Content { get; set; } = string.Empty;
    public SenderType SenderType { get; set; }

    // SP-02: AI analiz alanları
    public string? Intent { get; set; }
    public float? IntentConfidence { get; set; }
    public string? Sentiment { get; set; }
    public float? SentimentConfidence { get; set; }
    public int UrgencyLevel { get; set; } = 0; // 0-3

    // SP-02: Aksiyon takibi
    public string? ActionTaken { get; set; }      // "postponed_task:guid-123"
    public string? ActionSelected { get; set; }   // Kullanıcının seçtiği aksiyon butonu
    public Guid? RelatedTaskId { get; set; }       // FK → UserTask.Id (nullable)

    // SP-02: Proaktif mesaj desteği
    public bool IsProactive { get; set; } = false;

    // SP-02: Sunulan aksiyon butonları (JSON)
    public List<string>? ActionsOfferedJson { get; set; }
}