namespace EnvironmentsService.Src.Application.DTOs.GetRequest;

public class PaymentRequestDto
{
    public Guid ReservationId { get; set; }

    public string ClientEmail { get; set; } = string.Empty;

    public string ClientCI { get; set; } = "0";

    public string ClientNIT { get; set; } = "0";

    public string ClientFullName { get; set; } = string.Empty;

    public decimal TotalPrice { get; set; }
}