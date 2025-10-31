namespace EnvironmentsService.Src.Application.DTOs.Create;

public class CreateReservationDto
{
    public Guid EnvironmentId { get; set; }

    public Guid RenterId { get; set; }

    public List<TimeRangeDto> TimeRanges { get; set; } = [];

    public int TotalPrice { get; set; }

    public string Currency { get; set; } = "Bs.";

    public int PeopleQuantity { get; set; } = 1;
}
