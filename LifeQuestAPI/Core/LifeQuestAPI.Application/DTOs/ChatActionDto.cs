namespace LifeQuestAPI.Application.DTOs;

/// <summary>SP-02: Chatbot yanıtında sunulan aksiyon butonu.</summary>
public sealed record ChatActionDto
{
    public string Type { get; init; } = string.Empty;   // postpone_task | start_task | complete_task | abandon_task | select_task | activate_light_mode | show_stats
    public string Label { get; init; } = string.Empty;
    public Guid? TaskId { get; init; }
    public Dictionary<string, string>? Payload { get; init; }
}

/// <summary>SP-02: Kullanıcı bir aksiyon butonuna tıkladığında dönen sonuç.</summary>
public sealed record ChatActionResultDto
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public string? ActionType { get; init; }
    public Guid? AffectedTaskId { get; init; }
}
