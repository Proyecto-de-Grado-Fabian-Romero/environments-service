namespace EnvironmentsService.Src.Application.DTOs.Get;

public class WeeklyScheduleDto
{
    public int DayOfWeek { get; set; }

    public long StartTime { get; set; }

    public long EndTime { get; set; }
}
