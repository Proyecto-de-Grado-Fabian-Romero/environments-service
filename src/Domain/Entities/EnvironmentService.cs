namespace EnvironmentsService.Src.Domain.Entities;

public class EnvironmentService
{
    public Guid EnvironmentId { get; set; }

    public Guid ServiceId { get; set; }

    public Environment Environment { get; set; } = null!;

    public Service Service { get; set; } = null!;
}
