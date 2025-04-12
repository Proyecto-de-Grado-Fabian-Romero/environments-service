namespace EnvironmentsService.Src.Application.DTOs.Get;

public class EnvironmentAreaDto
{
    public EnvironmentDto Environment { get; set; } = null!;

    public AreaDto Area { get; set; } = null!;

    public int Quantity { get; set; } = 0;
}
