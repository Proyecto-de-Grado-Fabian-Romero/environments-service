namespace EnvironmentsService.Src.Application.DTOs.Get;

public class EnvironmentDto
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

    public Guid OwnerId { get; set; }

    public bool InstantBooking { get; set; }

    public int MinRentalTime { get; set; }

    public int MaxRentalTime { get; set; }

    public string RentalUnit { get; set; } = "hour";

    public string Equipment { get; set; } = string.Empty;

    public List<EnvironmentPhotoDto> Photos { get; set; } = [];

    public List<ServiceDto> Services { get; set; } = [];

    public List<EnvironmentAreaDto> EnvironmentAreas { get; set; } = [];

    public List<PricingPolicyDto> PricingPolicies { get; set; } = [];

    public List<DiscountPolicyDto> DiscountPolicies { get; set; } = [];

    public ICollection<WeeklyScheduleDto> WeeklySchedules { get; set; } = [];

    public ICollection<SpecialAvailabilityDto> SpecialAvailabilities { get; set; } = [];

    public List<string> PhotoUrls { get; set; } = [];
}
