using EnvironmentsService.Src.Application.DTOs.Responses;
using EnvironmentsService.Src.Application.Interfaces;
using EnvironmentsService.Src.Domain.Events;

namespace EnvironmentsService.Src.Infraestructure.Adapters;

public class ObjectDetectionAdapter(IMessageBus messageBus) : IObjectDetectionAdapter
{
    private readonly IMessageBus _messageBus = messageBus;

    public async Task<Dictionary<string, int>> DetectFromImagesAsync(List<string> imageUrls)
    {
        var request = new DetectionRequest
        {
            ImageUrls = imageUrls,
            ReplyTo = "detection_responses",
        };

        var response = await _messageBus.RequestAsync<DetectionRequest, DetectionResponse>(
            request,
            "detection_requests",
            "detection_responses"
        );

        return response.DetectedObjects;
    }
}
