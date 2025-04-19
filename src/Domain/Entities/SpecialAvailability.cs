namespace EnvironmentsService.Src.Domain.Entities;

public class SpecialAvailability
{
    public Guid Id { get; set; }

    public Guid EnvironmentId { get; set; }

    public DateTime Date { get; set; }

    public long StartTime { get; set; }

    public long EndTime { get; set; }

    public bool IsAvailable { get; set; }

    public Environment? Environment { get; set; }
}
