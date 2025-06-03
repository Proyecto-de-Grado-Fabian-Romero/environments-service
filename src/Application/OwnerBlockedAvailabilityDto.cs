namespace EnvironmentsService.Src.Application;

public class OwnerBlockedAvailabilityDto
{
    public string EnvironmentTitle { get; set; } = string.Empty;

    public string? EnvironmentPhotoUrl { get; set; }

    public long StartDate { get; set; }

    public long EndDate { get; set; }
}
