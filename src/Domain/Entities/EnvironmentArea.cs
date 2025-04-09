namespace EnvironmentsService.Src.Domain.Entities;

public class EnvironmentArea
{
    public Guid EnvironmentId { get; set; }

    public Guid AreaId { get; set; }

    public Environment Environment { get; set; } = null!;

    public Area Area { get; set; } = null!;
}
