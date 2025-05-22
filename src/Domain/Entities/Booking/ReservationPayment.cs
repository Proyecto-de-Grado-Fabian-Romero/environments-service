namespace EnvironmentsService.Src.Domain.Entities.Booking;

public class ReservationPayment
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ReservationRequestId { get; set; }

    public Reservation Reservation { get; set; } = null!;

    public decimal Amount { get; set; }

    public string Currency { get; set; } = "USD";

    public string Method { get; set; } = "card"; // QR, card, etc.

    public string Status { get; set; } = "pending"; // pending | paid | failed

    public long? PaidAt { get; set; }
}
