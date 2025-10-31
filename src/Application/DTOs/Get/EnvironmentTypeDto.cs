namespace EnvironmentsService.Src.Application.DTOs.Get;

public class EnvironmentTypeDto
{
    public string Name { get; set; } = string.Empty;

    public string PublicKey { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? IconUrl { get; set; }
}
