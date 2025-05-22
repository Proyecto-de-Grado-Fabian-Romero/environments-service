namespace EnvironmentsService.Src.Application.DTOs.Get;

public class TimeRange(long start, long end)
{
    public long Start { get; set; } = start;

    public long End { get; set; } = end;
}
