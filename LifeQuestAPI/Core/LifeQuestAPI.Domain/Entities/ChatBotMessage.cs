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

    
    public string? Intent { get; set; }
    public float? IntentConfidence { get; set; }
    public string? Sentiment { get; set; }
    public float? SentimentConfidence { get; set; }
    public int UrgencyLevel { get; set; } = 0; 

    
    public string? ActionTaken { get; set; }      
    public string? ActionSelected { get; set; }   
    public Guid? RelatedTaskId { get; set; }       

    
    public bool IsProactive { get; set; } = false;

    
    public List<string>? ActionsOfferedJson { get; set; }
}