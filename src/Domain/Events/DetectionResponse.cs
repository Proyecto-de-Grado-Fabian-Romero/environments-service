namespace EnvironmentsService.Src.Domain.Events;

public class DetectionResponse
{
    public string RequestId { get; set; } = string.Empty;

    public Dictionary<string, int> DetectedObjects { get; set; } = [];

    public bool Success { get; set; }

    public string Error { get; set; } = string.Empty;
}
