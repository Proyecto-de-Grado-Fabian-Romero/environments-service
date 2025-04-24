using EnvironmentsService.Src.Application.Interfaces;
using EnvironmentsService.Src.Domain.Entities.Tour360;
using EnvironmentsService.Src.Domain.Interfaces;

namespace EnvironmentsService.Src.Application.Services;

public class TourService(ITourRepository repository) : ITourService
{
    private readonly ITourRepository _repository = repository;

    public async Task<Tour> CreateTourAsync(List<Scene> scenes)
    {
        if (scenes == null || scenes.Count == 0)
        {
            throw new Exception("No se recibieron escenas.");
        }

        var tour = new Tour
        {
            Id = Guid.NewGuid().ToString(),
            Scenes = scenes,
        };

        await _repository.SaveAsync(tour);
        return tour;
    }

    public async Task<Tour?> GetTourByIdAsync(string id)
    {
        return await _repository.GetByIdAsync(id);
    }
}
