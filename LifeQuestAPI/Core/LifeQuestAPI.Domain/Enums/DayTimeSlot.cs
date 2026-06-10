namespace LifeQuestAPI.Domain.Enums;

public enum DayTimeSlot
{
    Morning = 0,   
    Noon = 1,      
    Afternoon = 2, 
    Evening = 3,   
    Night = 4      
}

public static class DayTimeSlotExtensions
{
    public static DayTimeSlot FromHour(int hour)
    {
        return hour switch
        {
            >= 6 and < 11 => DayTimeSlot.Morning,
            >= 11 and < 14 => DayTimeSlot.Noon,
            >= 14 and < 18 => DayTimeSlot.Afternoon,
            >= 18 and < 22 => DayTimeSlot.Evening,
            _ => DayTimeSlot.Night
        };
    }
}
