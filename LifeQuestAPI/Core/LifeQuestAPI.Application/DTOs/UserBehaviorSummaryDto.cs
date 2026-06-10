namespace LifeQuestAPI.Application.DTOs;


public class DayProgressDto
{
    public DateOnly Date { get; set; }
    public string DayName { get; set; } = string.Empty; 
    public int CompletedCount { get; set; }
    public int PostponedCount { get; set; }
    public int AbandonedCount { get; set; }
    public int TotalXpEarned { get; set; }
}


public class UserBehaviorSummaryDto
{
    
    public int AverageDailyActiveMinutes { get; set; }
    public string BestTimeOfDay { get; set; } = "Bilinmiyor"; 

    
    public string? MostCompletedCategory { get; set; }
    public string? MostSkippedCategory { get; set; }

    
    public double CompletionRateLast30Days { get; set; } 
    public int TotalCompletedLast30Days { get; set; }
    public int TotalPostponeCount { get; set; }
    public int AbandonedTaskCount { get; set; }

    
    public int CurrentStreak { get; set; }
    public int LongestStreak { get; set; }

    
    public List<DayProgressDto> WeeklyProgress { get; set; } = new();

    
    public string? InsightMessage { get; set; } 
    public List<string> AISystemInsights { get; set; } = new();
}
