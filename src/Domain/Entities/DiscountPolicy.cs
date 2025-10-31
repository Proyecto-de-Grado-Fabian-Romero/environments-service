namespace EnvironmentsService.Src.Domain.Entities;

public class DiscountPolicy
{
    public Guid Id { get; set; }

    public Guid EnvironmentId { get; set; }

    public int MinHours { get; set; }

    public decimal DiscountPercentage { get; set; }

    public Environment Environment { get; set; } = null!;
}
