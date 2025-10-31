namespace EnvironmentsService.Src.Application.DTOs.Responses;

public class PaymentStatusResponse
{
    required public string Status { get; set; } // "paid" o "pending"

    required public string InvoiceUrl { get; set; }

    public long PaidAt { get; set; }
}
