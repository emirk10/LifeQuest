namespace LifeQuestAPI.Domain.Enums;

public enum DayTimeSlot
{
    Morning = 0,   // 06:00 - 11:00
    Noon = 1,      // 11:00 - 14:00
    Afternoon = 2, // 14:00 - 18:00
    Evening = 3,   // 18:00 - 22:00
    Night = 4      // 22:00 - 06:00
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
