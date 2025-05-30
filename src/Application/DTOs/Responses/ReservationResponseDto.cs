using EnvironmentsService.Src.Application.DTOs.Create;

namespace EnvironmentsService.Src.Application.DTOs.Responses;

public class ReservationResponse
{
    public Guid PublicId { get; set; }

    public Guid EnvironmentId { get; set; }

    public string EnvironmentTitle { get; set; } = string.Empty;

    public string? EnvironmentPhotoUrl { get; set; }

    public Guid RenterId { get; set; }

    public List<TimeRangeDto> TimeRanges { get; set; } = [];

    public string Status { get; set; } = default!;

    public bool IsInstant { get; set; }

    public decimal TotalPrice { get; set; }

    public string Currency { get; set; } = "Bs.";

    public List<ReservationPaymentResponse> Payments { get; set; } = [];
}
