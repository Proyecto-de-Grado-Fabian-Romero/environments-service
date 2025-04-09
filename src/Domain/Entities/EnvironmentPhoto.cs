namespace EnvironmentsService.Src.Domain.Entities;

public class EnvironmentPhoto
{
    public Guid Id { get; set; }

    public Guid EnvironmentId { get; set; }

    public Guid FileId { get; set; }

    required public string FileName { get; set; }

    public int Order { get; set; }

    public string? Caption { get; set; }

    public Environment Environment { get; set; } = null!;
}
