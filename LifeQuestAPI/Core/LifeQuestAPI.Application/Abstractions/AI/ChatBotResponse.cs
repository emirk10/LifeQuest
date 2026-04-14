namespace LifeQuestAPI.Application.Abstractions.AI;

/// <summary>
/// Python AI servisinden dönen chatbot cevabını temsil eder.
/// </summary>
public class ChatBotResponse
{
    public string Intent { get; set; } = string.Empty;
    public float Confidence { get; set; }
    public string Response { get; set; } = string.Empty;
    public string Sentiment { get; set; } = string.Empty;
    public float SentimentConfidence { get; set; }
    public List<string> Suggestions { get; set; } = new();
}
