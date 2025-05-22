using EnvironmentsService.Src.Domain.Entities.Booking;

namespace EnvironmentsService.Src.Domain.Entities;

public class NonAvailability
{
    public Guid Id { get; set; }

    public Guid EnvironmentId { get; set; }

    public long StartDate { get; set; }

    public long EndDate { get; set; }

    public string Type { get; set; } = "OwnerBlocked"; // "Reservation"

    public Guid? ReservationId { get; set; }

    public Environment Environment { get; set; } = null!;

    public Reservation? Reservation { get; set; }
}
