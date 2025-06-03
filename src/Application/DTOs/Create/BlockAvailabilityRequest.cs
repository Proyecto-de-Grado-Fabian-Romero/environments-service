namespace EnvironmentsService.src.Application.DTOs.Create;

public class BlockAvailabilityRequest
{
    public Guid EnvironmentId { get; set; }

    public long StartDate { get; set; }

    public long EndDate { get; set; }
}
