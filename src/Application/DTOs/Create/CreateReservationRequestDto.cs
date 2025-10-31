namespace EnvironmentsService.Src.Application.DTOs.Create;

public class CreateReservationRequest
{
    public Guid EnvironmentId { get; set; }

    public List<TimeRangeDto> TimeRanges { get; set; } = [];

    public int TotalPrice { get; set; }

    public string Currency { get; set; } = "Bs.";
}
