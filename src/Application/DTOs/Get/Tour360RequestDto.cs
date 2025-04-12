namespace EnvironmentsService.Src.Application.DTOs.Get;

public class Tour360RequestDto
{
    public Guid Id { get; set; }

    public Guid EnvironmentId { get; set; }

    public long RequestDate { get; set; }

    public long? ScheduledDate { get; set; }

    public string Status { get; set; } = "pending";
}
