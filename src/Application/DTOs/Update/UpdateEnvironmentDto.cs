namespace EnvironmentsService.Src.Application.DTOs.Update;

using System.Text.Json;
using EnvironmentsService.Src.Application.DTOs.Create;
using EnvironmentsService.Src.Application.DTOs.Get;

public class UpdateEnvironmentDto
{
    public string? Title { get; set; }

    public string? Description { get; set; }

    public string? Location { get; set; }

    public double? Latitude { get; set; }

    public double? Longitude { get; set; }

    public string? TypePublicKey { get; set; }

    public string? RentalUnit { get; set; } // "Horas" | "Días"

    public int? Capacity { get; set; }

    public bool? InstantBooking { get; set; }

    public int? MinRentalTime { get; set; }

    public int? MaxRentalTime { get; set; }

    public bool? Request360Tour { get; set; }

    // Archivos nuevos
    public List<IFormFile> Images { get; set; } = [];

    // JSON crudos (opcionales)
    public string? EquipmentJson { get; set; } // string JSON

    public string? PricingPoliciesJson { get; set; } // List<PricingPolicyDto>

    public string? DiscountPoliciesJson { get; set; } // List<DiscountPolicyDto>

    public string? WeeklySchedulesJson { get; set; } // List<WeeklyScheduleDto>

    public string? AreasJson { get; set; } // List<AreaQuantityDto>

    public string? ServicePublicKeysJson { get; set; } // List<string>

    // Imágenes existentes
    // Modo A: lista completa de las que quieres conservar
    public string? KeepPhotoIdsJson { get; set; } // List<string>

    // Modo B: incrementales para borrar
    public string? DeletePhotoIdsJson { get; set; } // List<string>

    // Helpers tipados (igual que en Create)
    public List<PricingPolicyDto>? PricingPolicies =>
        string.IsNullOrWhiteSpace(PricingPoliciesJson)
            ? null : JsonSerializer.Deserialize<List<PricingPolicyDto>>(PricingPoliciesJson!)!;

    public List<DiscountPolicyDto>? DiscountPolicies =>
        string.IsNullOrWhiteSpace(DiscountPoliciesJson)
            ? null : JsonSerializer.Deserialize<List<DiscountPolicyDto>>(DiscountPoliciesJson!)!;

    public List<WeeklyScheduleDto>? WeeklySchedules =>
        string.IsNullOrWhiteSpace(WeeklySchedulesJson)
            ? null : JsonSerializer.Deserialize<List<WeeklyScheduleDto>>(WeeklySchedulesJson!)!;

    public List<AreaQuantityDto>? Areas =>
        string.IsNullOrWhiteSpace(AreasJson)
            ? null : JsonSerializer.Deserialize<List<AreaQuantityDto>>(AreasJson!)!;

    public List<string>? ServicePublicKeys =>
        string.IsNullOrWhiteSpace(ServicePublicKeysJson)
            ? null : JsonSerializer.Deserialize<List<string>>(ServicePublicKeysJson!)!;

    public List<string>? KeepPhotoIds =>
        string.IsNullOrWhiteSpace(KeepPhotoIdsJson)
            ? null : JsonSerializer.Deserialize<List<string>>(KeepPhotoIdsJson!)!;

    public List<string>? DeletePhotoIds =>
        string.IsNullOrWhiteSpace(DeletePhotoIdsJson)
            ? null : JsonSerializer.Deserialize<List<string>>(DeletePhotoIdsJson!)!;
}
