using EnvironmentsService.Src.Domain.Entities.Tour360;

namespace EnvironmentsService.Src.Application.Interfaces;

public interface ITourService
{
    Task<Tour> CreateTourAsync(List<IFormFile> files, string jsonScenes);

    Task<Tour?> GetTourByIdAsync(string id);
}
