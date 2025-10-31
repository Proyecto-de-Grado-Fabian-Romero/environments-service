namespace EnvironmentsService.Src.Application.DTOs.Get;

public class SpecialAvailabilityDto
{
    public DateTime Date { get; set; }

    public long StartTime { get; set; }

    public long EndTime { get; set; }

    public bool IsAvailable { get; set; }
}
