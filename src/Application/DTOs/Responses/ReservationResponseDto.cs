namespace EnvironmentsService.Src.Application.DTOs.Responses;

public class ReservationResponse
{
    public Guid Id { get; set; }

    public Guid EnvironmentId { get; set; }

    public Guid RenterId { get; set; }

    public long StartDate { get; set; }

    public long EndDate { get; set; }

    public string Status { get; set; } = default!;

    public bool IsInstant { get; set; }
}
