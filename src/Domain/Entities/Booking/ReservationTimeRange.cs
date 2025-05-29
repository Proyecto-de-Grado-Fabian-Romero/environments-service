namespace EnvironmentsService.Src.Domain.Entities.Booking;

public class ReservationTimeRange
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ReservationId { get; set; }

    public Reservation Reservation { get; set; } = null!;

    public long StartDate { get; set; }

    public long EndDate { get; set; }
}
