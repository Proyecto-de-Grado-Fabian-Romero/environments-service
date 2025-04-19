namespace EnvironmentsService.Src.Domain.Entities;

public class Service
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public ICollection<EnvironmentService> EnvironmentServices { get; set; } = [];

    public string PublicKey { get; set; } = string.Empty;
}
