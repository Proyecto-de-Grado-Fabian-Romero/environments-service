namespace EnvironmentsService.Src.Application.DTOs.Get;

public class PricingPolicyDto
{
    public decimal BasePrice { get; set; }

    public string Currency { get; set; } = string.Empty;

    public string PriceUnit { get; set; } = string.Empty;

    public decimal? ExtraGuestPrice { get; set; }
}
