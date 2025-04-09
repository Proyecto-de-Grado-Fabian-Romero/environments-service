namespace EnvironmentsService.Src.Domain.Entities;

public class PricingPolicy
{
    public Guid Id { get; set; }

    public Guid EnvironmentId { get; set; }

    public decimal BasePrice { get; set; }

    public string Currency { get; set; } = string.Empty;

    public string PriceUnit { get; set; } = string.Empty;

    public decimal? ExtraGuestPrice { get; set; }

    public Environment Environment { get; set; } = null!;
}
