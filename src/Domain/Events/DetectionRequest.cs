namespace EnvironmentsService.Src.Domain.Events;

public class DetectionRequest
{
    public string RequestId { get; set; } = Guid.NewGuid().ToString();

    public List<string> ImageUrls { get; set; } = [];

    public string ReplyTo { get; set; } = string.Empty;
}
