namespace LifeQuestAPI.Application.Abstractions.AI;

/// <summary>
/// Python AI servisinden dönen görüntü doğrulama sonucunu temsil eder.
/// </summary>
public class ImageVerificationResult
{
    public bool IsValid { get; set; }
    public float Confidence { get; set; }
    public List<DetectedLabel> DetectedLabels { get; set; } = new();
    public string ExpectedCategory { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public class DetectedLabel
{
    public string Label { get; set; } = string.Empty;
    public float Confidence { get; set; }
}
