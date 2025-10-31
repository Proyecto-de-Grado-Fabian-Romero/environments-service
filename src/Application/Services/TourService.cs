using System.Text.Json;
using EnvironmentsService.Src.Application.Interfaces;
using EnvironmentsService.Src.Domain.Entities.Tour360;
using EnvironmentsService.Src.Domain.Interfaces;

namespace EnvironmentsService.Src.Application.Services;

public class TourService(
    ITourRepository repository,
    IEnvironmentRepository environmentRepository,
    IObjectDetectionAdapter objectDetectionAdapter
) : ITourService
{
    private readonly ITourRepository _repository = repository;
    private readonly IEnvironmentRepository _environmentRepository = environmentRepository;
    private readonly IObjectDetectionAdapter _objectDetectionAdapter = objectDetectionAdapter;

    public async Task<Tour> CreateTourAsync(Guid environmentPublicId, List<Scene> scenes)
    {
        if (scenes == null || scenes.Count == 0)
        {
            throw new Exception("No se recibieron escenas.");
        }

        var tour = new Tour
        {
            Id = Guid.NewGuid().ToString(),
            Scenes = scenes,
            CreatedDate = DateTimeOffset
                .UtcNow.ToOffset(new TimeSpan(-4, 0, 0))
                .ToUnixTimeSeconds(),
        };

        await _repository.SaveAsync(tour);

        var environment =
            await _environmentRepository.GetSingleEnvironment(environmentPublicId)
            ?? throw new Exception("Ambiente no encontrado.");

        environment.Tour360Id = Guid.Parse(tour.Id);
        await _environmentRepository.SaveChangesAsync();

        var imageUrls = scenes
            .Where(s => !string.IsNullOrWhiteSpace(s.FileUrl))
            .Select(s => s.FileUrl)
            .ToList();

        if (imageUrls.Count > 0)
        {
            var detectedObjects = await _objectDetectionAdapter.DetectFromImagesAsync(imageUrls);
            await _environmentRepository.UpdateDetectedEquipmentAsync(
                environmentPublicId,
                JsonSerializer.Serialize(detectedObjects)
            );
        }

        return tour;
    }

    public async Task<Tour?> GetTourByIdAsync(string id)
    {
        return await _repository.GetByIdAsync(id);
    }
}
