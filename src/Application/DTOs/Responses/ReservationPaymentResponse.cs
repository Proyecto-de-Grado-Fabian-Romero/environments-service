namespace EnvironmentsService.Src.Application.DTOs.Responses;

public class ReservationPaymentResponse
{
    public decimal Amount { get; set; }

    public string Currency { get; set; } = "USD";

    public string Method { get; set; } = "card";

    public string Status { get; set; } = "pending";

    public long? PaidAt { get; set; }
}
