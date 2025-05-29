using System.Text.Json;
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

    required public string RentalUnit { get; set; }

    public int Capacity { get; set; }

    public bool InstantBooking { get; set; }

    public int MinRentalTime { get; set; }

    public int MaxRentalTime { get; set; }

    public bool Request360Tour { get; set; }

    // Archivos
    public List<IFormFile> Images { get; set; } = [];

    // JSON crudo que llegará como string desde FormData
    public string? EquipmentJson { get; set; }

    public string? PricingPoliciesJson { get; set; }

    public string? DiscountPoliciesJson { get; set; }

    public string? WeeklySchedulesJson { get; set; }

    public string? AreasJson { get; set; }

    public string? ServicePublicKeysJson { get; set; }

    public List<PricingPolicyDto> PricingPolicies =>
        string.IsNullOrWhiteSpace(PricingPoliciesJson)
            ? []
            : JsonSerializer.Deserialize<List<PricingPolicyDto>>(PricingPoliciesJson)!;

    public List<DiscountPolicyDto> DiscountPolicies =>
        string.IsNullOrWhiteSpace(DiscountPoliciesJson)
            ? []
            : JsonSerializer.Deserialize<List<DiscountPolicyDto>>(DiscountPoliciesJson)!;

    public List<WeeklyScheduleDto> WeeklySchedules =>
        string.IsNullOrWhiteSpace(WeeklySchedulesJson)
            ? []
            : JsonSerializer.Deserialize<List<WeeklyScheduleDto>>(WeeklySchedulesJson)!;

    public List<AreaQuantityDto> Areas =>
        string.IsNullOrWhiteSpace(AreasJson)
            ? []
            : JsonSerializer.Deserialize<List<AreaQuantityDto>>(AreasJson)!;

    public List<string> ServicePublicKeys =>
        string.IsNullOrWhiteSpace(ServicePublicKeysJson)
            ? []
            : JsonSerializer.Deserialize<List<string>>(ServicePublicKeysJson)!;
}
