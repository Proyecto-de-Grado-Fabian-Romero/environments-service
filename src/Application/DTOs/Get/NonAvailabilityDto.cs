namespace EnvironmentsService.Src.Application.DTOs.Get;

public class NonAvailabilityDto
{
    public long StartDate { get; set; }

    public long EndDate { get; set; }

    public string Type { get; set; } = "OwnerBlocked"; // "Reservation"

    public Guid? ReservationId { get; set; }
}
