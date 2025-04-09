namespace EnvironmentsService.Src.Domain.Entities;

public class Environment
{
    public Guid Id { get; set; }

    public Guid PublicId { get; set; }

    required public string Title { get; set; }

    required public string Description { get; set; }

    public Guid TypeId { get; set; }

    public int Capacity { get; set; }

    required public string Location { get; set; }

    public decimal Latitude { get; set; }

    public decimal Longitude { get; set; }

    required public string Equipment { get; set; }

    public Guid? Tour360Id { get; set; }

    public Guid OwnerId { get; set; }

    public bool InstantBooking { get; set; }

    public int MinRentalTime { get; set; }

    public int MaxRentalTime { get; set; }

    required public string RentalUnit { get; set; }

    required public EnvironmentType Type { get; set; }

    public ICollection<EnvironmentPhoto> Photos { get; set; } = [];

    public ICollection<PricingPolicy> PricingPolicies { get; set; } = [];

    public ICollection<DiscountPolicy> DiscountPolicies { get; set; } = [];

    public ICollection<Availability> Availabilities { get; set; } = [];

    public ICollection<EnvironmentService> EnvironmentServices { get; set; } = [];

    public ICollection<EnvironmentArea> EnvironmentAreas { get; set; } = [];
}
