namespace EnvironmentsService.Src.Application.DTOs.Create;

public class CreatePaymentWithClientDto
{
    public Guid ReservationId { get; set; }

    public string ClientEmail { get; set; } = string.Empty;

    public string ClientFullName { get; set; } = string.Empty;

    public string ClientCI { get; set; } = string.Empty;

    public string ClientNIT { get; set; } = string.Empty;
}
