namespace EnvironmentsService.Src.Domain.Entities.Booking;

public class Reservation
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid EnvironmentId { get; set; }

    public Environment Environment { get; set; } = null!;

    public Guid RenterId { get; set; }

    public Guid OwnerId { get; set; }

    public long StartDate { get; set; }

    public long EndDate { get; set; }

    public decimal TotalPrice { get; set; }

    public string Currency { get; set; } = "Bs.";

    public bool IsInstant { get; set; }

    public string Status { get; set; } = "pending"; // pending | confirmed | rejected | cancelled

    public long CreatedAt { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    public ICollection<ReservationPayment> Payments { get; set; } = [];

    public ICollection<NonAvailability> BlockedSlots { get; set; } = [];
}
