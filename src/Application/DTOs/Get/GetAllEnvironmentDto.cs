namespace EnvironmentsService.Src.Application.DTOs.Get;

public class GetAllEnvironmentDto
{
    public Guid PublicId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public decimal Latitude { get; set; }

    public decimal Longitude { get; set; }

    public int Capacity { get; set; }

    public EnvironmentTypeDto Type { get; set; } = null!;

    public Guid? Tour360Id { get; set; }

    public bool InstantBooking { get; set; }

    public int MinRentalTime { get; set; }

    public int MaxRentalTime { get; set; }

    public string RentalUnit { get; set; } = "hour";

    public List<string> PhotoUrls { get; set; } = [];

    public List<PricingPolicyDto> PricingPolicies { get; set; } = [];

    public long? LastTour360Date { get; set; }
}
