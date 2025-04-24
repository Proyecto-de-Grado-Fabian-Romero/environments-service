using EnvironmentsService.Src.Application.DTOs.Get;

namespace EnvironmentsService.Src.Application.DTOs.Create;

public class CreateEnvironmentDto
{
    required public string Title { get; set; }

    required public string Description { get; set; }

    required public string Location { get; set; }

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    required public string TypePublicKey { get; set; }

    public List<string> ServicePublicKeys { get; set; } = [];

    public int Capacity { get; set; }

    public bool InstantBooking { get; set; }

    public int MinRentalTime { get; set; }

    public int MaxRentalTime { get; set; }

    required public string RentalUnit { get; set; } // "Horas" or "Días"

    public List<AreaQuantityDto> Areas { get; set; } = [];

    public List<IFormFile> Images { get; set; } = [];

    required public string? EquipmentJson { get; set; }

    public List<PricingPolicyDto> PricingPolicies { get; set; } = [];

    public List<DiscountPolicyDto> DiscountPolicies { get; set; } = [];

    public List<WeeklyScheduleDto> WeeklySchedules { get; set; } = [];

    public bool Request360Tour { get; set; }
}
