namespace EnvironmentsService.Src.Application.DTOs.Responses;

public class OwnerBlockedAvailabilityDto
{
    public Guid EnvironmentId { get; set; } = Guid.NewGuid();

    public string EnvironmentTitle { get; set; } = string.Empty;

    public string? EnvironmentPhotoUrl { get; set; }

    public long StartDate { get; set; }

    public long EndDate { get; set; }
}
