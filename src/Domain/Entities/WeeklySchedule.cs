namespace EnvironmentsService.Src.Domain.Entities;

public class WeeklySchedule
{
    public Guid Id { get; set; }

    public Guid EnvironmentId { get; set; }

    public int DayOfWeek { get; set; }

    public long StartTime { get; set; } // In minutes

    public long EndTime { get; set; }

    public Environment? Environment { get; set; }
}
