namespace EnvironmentsService.Src.Domain.Entities;

public class EnvironmentType
{
    public Guid Id { get; set; }

    required public string Name { get; set; }
}
