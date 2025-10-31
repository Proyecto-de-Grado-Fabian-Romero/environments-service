using EnvironmentsService.Src.Application.DTOs.Create;

namespace EnvironmentsService.Src.Domain.Events;

public record GetEnvironmentDetailsMessage(Guid EnvironmentPublicId, string CorrelationId);

public record UpdateDetectedObjectsMessage(
    Guid EnvironmentPublicId,
    Dictionary<string, int> DetectedObjects
);

public record UploadTourMessage(
    Guid EnvironmentPublicId,
    TourUploadDto TourUpload,
    string CorrelationId
);

public record UploadTourResponse
{
    public bool Success { get; set; }
    public string Error { get; set; } = string.Empty;
}
