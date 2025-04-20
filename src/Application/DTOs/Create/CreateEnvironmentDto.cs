using EnvironmentsService.Src.Application.DTOs.Get;

namespace EnvironmentsService.Src.Application.DTOs.Create;

public class CreateEnvironmentDto
{
    required public string Name { get; set; }

    required public string Description { get; set; }

    required public string Location { get; set; }

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    required public string TypePublicKey { get; set; }

    public List<string> ServicePublicKeys { get; set; } = [];

    public List<AreaQuantityDto> Areas { get; set; } = [];

    public List<IFormFile> Images { get; set; } = [];

    required public string? EquipmentJson { get; set; }

    public List<PricingPolicyDto> PricingPolicies { get; set; } = [];

    public List<DiscountPolicyDto> DiscountPolicies { get; set; } = [];

    public List<WeeklyScheduleDto> WeeklySchedules { get; set; } = [];

    public bool Request360Tour { get; set; }
}
