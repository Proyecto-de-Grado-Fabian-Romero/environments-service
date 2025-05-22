namespace EnvironmentsService.Src.Application.DTOs.Create;

public class CreateReservationRequest
{
    public Guid EnvironmentId { get; set; }

    public Guid RenterId { get; set; }

    public long StartDate { get; set; }

    public long EndDate { get; set; }
}
