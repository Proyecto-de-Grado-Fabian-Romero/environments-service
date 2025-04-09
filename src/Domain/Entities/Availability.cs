namespace EnvironmentsService.Src.Domain.Entities;

public class Availability
{
    public Guid EnvironmentId { get; set; }

    public DateOnly Date { get; set; }

    public bool IsAvailable { get; set; }

    public Environment Environment { get; set; } = null!;
}
