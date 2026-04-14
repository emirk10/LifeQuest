using LifeQuestAPI.Domain.Entities.Common;
using LifeQuestAPI.Domain.Enums;

namespace LifeQuestAPI.Domain.Entities;

public sealed class TaskVerification : BaseEntity
{
    public Guid UserTaskId { get; set; }
    public UserTask UserTask { get; set; } = default!;

    public VerificationType Type { get; set; }
    public VerificationStatus Status { get; set; } = VerificationStatus.Pending;

    // Fotoğraf doğrulama
    public string? PhotoPath { get; set; }
    public float? AIConfidenceScore { get; set; }
    public string? AIAnalysisResult { get; set; }

    // Metin/yansıma doğrulama
    public string? ReflectionText { get; set; }
    public int? ReflectionWordCount { get; set; }
    public float? SentimentScore { get; set; }
    public bool? IsReflectionRelevant { get; set; }

    // Zaman doğrulama
    public DateTime? TimerStartedAt { get; set; }
    public DateTime? TimerEndedAt { get; set; }
    public int? MinimumMinutesRequired { get; set; }

    // Quiz doğrulama
    public int? QuizScore { get; set; }
    public int? QuizTotalQuestions { get; set; }

    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    public string? ReviewNotes { get; set; }
}
