namespace EnvironmentsService.Src.Domain.Entities;

public class Area
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public ICollection<EnvironmentArea> EnvironmentAreas { get; set; } = [];

    public string PublicKey { get; set; } = string.Empty;
}
