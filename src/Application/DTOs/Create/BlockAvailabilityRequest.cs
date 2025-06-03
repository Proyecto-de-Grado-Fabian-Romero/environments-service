namespace EnvironmentsService.src.Application.DTOs.Create;

public class BlockAvailabilityRequest
{
    public Guid EnvironmentId { get; set; }

    public long Date { get; set; }
}
