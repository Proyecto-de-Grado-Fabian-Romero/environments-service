using EnvironmentsService.Src.Application.DTOs.Create;

namespace EnvironmentsService.Src.Application.DTOs.Responses;

public class ReservationResponse
{
    public Guid PublicId { get; set; }

    public Guid EnvironmentId { get; set; }

    public Guid EnvironmentPublicId { get; set; }

    public string EnvironmentTitle { get; set; } = string.Empty;

    public string RentalUnit { get; set; } = string.Empty;

    public string? EnvironmentPhotoUrl { get; set; }

    public Guid RenterId { get; set; }

    public Guid OwnerId { get; set; }

    public List<TimeRangeDto> TimeRanges { get; set; } = [];

    public string Status { get; set; } = default!;

    public bool IsInstant { get; set; }

    public decimal TotalPrice { get; set; }

    public string Currency { get; set; } = "Bs.";

    public List<ReservationPaymentResponse> Payments { get; set; } = [];

    public int PeopleQuantity { get; set; } = 1;

    public long CreatedAt { get; set; } =
    DateTimeOffset.UtcNow.ToOffset(new TimeSpan(-4, 0, 0)).ToUnixTimeSeconds();

    public long ConfirmedAt { get; set; } =
        DateTimeOffset.UtcNow.ToOffset(new TimeSpan(-4, 0, 0)).ToUnixTimeSeconds();
}
