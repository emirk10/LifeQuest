namespace LifeQuestAPI.Application.Abstractions.AI;

/// <summary>
/// Python AI servisinden dönen yansıma metni doğrulama sonucunu temsil eder.
/// </summary>
public class ReflectionVerificationResult
{
    public bool IsValid { get; set; }
    public int WordCount { get; set; }
    public float QualityScore { get; set; }
    public float RelevanceScore { get; set; }
    public bool SpamDetected { get; set; }
    public List<string> Issues { get; set; } = new();
    public string Message { get; set; } = string.Empty;
}
